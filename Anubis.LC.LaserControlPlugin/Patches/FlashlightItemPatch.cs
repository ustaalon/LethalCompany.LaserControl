using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    public class FlashlightItemPatch
    {
        static string LASER_PROP_NAME = "LaserPointer";
        static Turret? PrevUsedTurret;

        [HarmonyPatch("ItemActivate")]
        [HarmonyPrefix]
        private static void ItemActivate(FlashlightItem __instance, bool used, bool buttonDown = true)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME)) return;
            if (!Networking.Instance.GetConfigItemValueOfPlayer(nameof(LethalConfigHelper.IsPointerCanControlTurrets))) return;

            if (__instance.gameObject.GetComponent<LaserPointerRaycast>() == null)
            {
                __instance.gameObject.AddComponent<LaserPointerRaycast>();
                FlashlightItemExtensions.LaserPointerRaycastCurrentInstance = __instance.GetComponent<LaserPointerRaycast>();
            }
            else
            {
                FlashlightItemExtensions.LaserPointerRaycastCurrentInstance = __instance.GetComponent<LaserPointerRaycast>();
                if (FlashlightItemExtensions.LaserPointerRaycastCurrentInstance && !FlashlightItemExtensions.LaserPointerRaycastCurrentInstance.state)
                {
                    Object.Destroy(FlashlightItemExtensions.LaserPointerRaycastCurrentInstance);
                }
            }
            Networking.Instance.SyncAllTurretsAndRaycastsServerRpc();
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update(FlashlightItem __instance)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME)) return;
            if (!Networking.Instance.GetConfigItemValueOfPlayer(nameof(LethalConfigHelper.IsPointerCanControlTurrets))) return;

            LaserPointerRaycast laserPointerRaycast = __instance.GetComponent<LaserPointerRaycast>();
            if (!laserPointerRaycast) return;

            if (laserPointerRaycast.state)
            {
                ModStaticHelper.Logger.LogInfo("Pointer is working");
                var turret = Networking.Instance.GetNearestTurret();
                if (turret == null) return;
                ModStaticHelper.Logger.LogInfo("Pointer is working and turret found");

                __instance.UseLaserPointerItemBatteries();

                if (PrevUsedTurret && PrevUsedTurret?.NetworkObjectId != turret.NetworkObjectId && PrevUsedTurret?.turretMode != TurretMode.Detection)
                {
                    ModStaticHelper.Logger.LogInfo("Previous turret no longer in player control, stop firing (ON)");
                    Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                }
                Networking.Instance.TurnTowardsLaserBeamIfHasLOSServerRpc(turret.NetworkObjectId, laserPointerRaycast.GetHashCode());

                PrevUsedTurret = turret;
                __instance.DestroyIfBatteryIsEmpty(turret);
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
    }
}
