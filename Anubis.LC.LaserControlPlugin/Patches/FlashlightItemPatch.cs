﻿using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine.InputSystem;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    public class FlashlightItemPatch
    {
        static string LASER_PROP_NAME = "LaserPointer";
        static Turret? PrevUsedTurret;
        static float counterOfUsage = 0f;
        static float maxSeconds = 15f;
        private static InputAction leftAltAction;
        private static InputAction rightClickAction;
        private static bool IsClickToShoot = false;
        private static bool IsBusy = false;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(FlashlightItem __instance)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME)) return;
            if (ModStaticHelper.IsThisModInstalled("FlipMods.ReservedFlashlightSlot"))
            {
                leftAltAction = new InputAction(binding: "<Keyboard>/leftAlt");
                leftAltAction.canceled += context => OnAltKeyReleased(__instance);
                leftAltAction.Enable();
            }

            rightClickAction = new InputAction(binding: "<Mouse>/rightButton");
            rightClickAction.canceled += context => OnRightClickReleased(__instance);
            rightClickAction.Enable();
        }

        [HarmonyPatch("ItemActivate")]
        [HarmonyPrefix]
        private static void ItemActivate(FlashlightItem __instance, bool used, bool buttonDown = true)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME)) return;

            FlashlightItemExtensions.LaserPointerRaycastCurrentInstance = __instance.GetComponent<LaserPointerRaycast>();

            if (FlashlightItemExtensions.LaserPointerRaycastCurrentInstance == null)
            {
                __instance.gameObject.AddComponent<LaserPointerRaycast>();
            }

            if (FlashlightItemExtensions.LaserPointerRaycastCurrentInstance && !FlashlightItemExtensions.LaserPointerRaycastCurrentInstance.state)
            {
                ModStaticHelper.Logger.LogInfo("Laser pointer destroyed");
                Object.Destroy(FlashlightItemExtensions.LaserPointerRaycastCurrentInstance);
            }
            Networking.Instance.SyncAllTurretsAndRaycastsServerRpc();
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update(FlashlightItem __instance)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME)) return;
            if (!Networking.Instance.GetConfigItemValueOfPlayer<bool>(nameof(LethalConfigHelper.IsPointerCanControlTurrets))) return;

            LaserPointerRaycast laserPointerRaycast = __instance.GetComponent<LaserPointerRaycast>();
            if (!laserPointerRaycast) return;

            if (laserPointerRaycast.state)
            {
                ModStaticHelper.Logger.LogInfo("Pointer is working");
                var turret = Networking.Instance.GetNearestTurret();
                if (turret == null) return;
                __instance.UseLaserPointerItemBatteries(turret);
                ModStaticHelper.Logger.LogInfo("Pointer is working and turret found");

                if (PrevUsedTurret && PrevUsedTurret?.NetworkObjectId != turret.NetworkObjectId && PrevUsedTurret?.turretMode != TurretMode.Detection)
                {
                    ModStaticHelper.Logger.LogInfo("Previous turret no longer in player control, stop firing (ON)");
                    Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                }
                counterOfUsage += Time.deltaTime;

                if (counterOfUsage < maxSeconds)
                {
                    if (!IsClickToShoot)
                    {
                        ModStaticHelper.Logger.LogInfo("Charging...");
                        Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                        Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Charging);
                    }
                    else
                    {
                        ModStaticHelper.Logger.LogInfo("Firing...");
                        Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Firing);
                    }
                    Networking.Instance.TurnTowardsLaserBeamIfHasLOSServerRpc(turret.NetworkObjectId, laserPointerRaycast.GetHashCode());
                    IsBusy = false;
                }
                else
                {
                    __instance.SwitchFlashlight(on: false);
                    turret.TurnOffAndOnTurret();
                    counterOfUsage = 0f;
                }

                PrevUsedTurret = turret;
                __instance.DestroyIfBatteryIsEmpty(turret);
            }
            else
            {
                ModStaticHelper.Logger.LogInfo("Laser pointer destroyed");
                Object.Destroy(laserPointerRaycast);
            }

            if (!laserPointerRaycast.state && PrevUsedTurret != null)
            {
                if (PrevUsedTurret.turretMode != TurretMode.Detection)
                {
                    ModStaticHelper.Logger.LogInfo("Previous turret no longer in player control, stop firing (OFF)");
                    Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                }

                __instance.DestroyIfBatteryIsEmpty(PrevUsedTurret);
                PrevUsedTurret = null;
            }
        }

        private static void OnRightClickReleased(FlashlightItem __instance)
        {
            if (IsBusy) return;
            LaserPointerRaycast laserPointerRaycast = __instance.GetComponent<LaserPointerRaycast>();
            if (!laserPointerRaycast || !laserPointerRaycast.state) return;
            IsBusy = true;
            IsClickToShoot = IsClickToShoot == false;
        }

        private static void OnAltKeyReleased(FlashlightItem __instance)
        {
            __instance.SwitchFlashlight(on: false);
        }
    }
}
