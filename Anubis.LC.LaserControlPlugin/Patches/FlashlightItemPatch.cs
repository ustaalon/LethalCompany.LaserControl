using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using HarmonyLib;
using UnityEngine;

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
            if (!__instance.name.Contains(LASER_PROP_NAME) || !LethalConfigHelper.IsBeta.Value) return;

            if (__instance.gameObject.GetComponent<LaserPointerRaycast>() == null)
            {
                __instance.gameObject.AddComponent<LaserPointerRaycast>();
            }
            else
            {
                LaserPointerRaycast laserPointerRaycast = __instance.GetComponent<LaserPointerRaycast>();
                if (laserPointerRaycast && !laserPointerRaycast.state)
                {
                    Object.Destroy(laserPointerRaycast);
                }
            }
            Networking.Instance.SyncAllTurretsAndRaycastsServerRpc();
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update(FlashlightItem __instance)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME) || !LethalConfigHelper.IsBeta.Value) return;

            LaserPointerRaycast laserPointerRaycast = __instance.GetComponent<LaserPointerRaycast>();
            if (!laserPointerRaycast) return;

            if (laserPointerRaycast.state)
            {
                var turret = Networking.Instance.GetNearestTurret();
                if (turret == null) return;

                __instance.UseLaserPointerItemBatteries();

                if (PrevUsedTurret && PrevUsedTurret?.NetworkObjectId != turret.NetworkObjectId)
                {
                    Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                }
                Networking.Instance.TurnTowardsLaserBeamIfHasLOSServerRpc(turret.NetworkObjectId, laserPointerRaycast.GetHashCode());

                PrevUsedTurret = turret;
            }

            if (!laserPointerRaycast.state && PrevUsedTurret != null)
            {
                Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                PrevUsedTurret = null;
            }
        }
    }
}
