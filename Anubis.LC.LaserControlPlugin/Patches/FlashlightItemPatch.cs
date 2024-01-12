using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    public class FlashlightItemPatch
    {
        static string LASER_PROP_NAME = "LaserPointer";
        static Turret? PrevUsedTurret;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update(FlashlightItem __instance)
        {
            if (!__instance.name.Contains(LASER_PROP_NAME)) return;

            LaserPointerRaycast laserPointerRaycast = Object.FindObjectOfType<LaserPointerRaycast>();
            if (!laserPointerRaycast) return;

            if (laserPointerRaycast.state)
            {
                var turret = __instance.CheckForNearestTurret();
                if (turret == null) return;

                __instance.UseLaserPointerItemBatteries();
                PluginNetworkingInstance.Instance.TurnTowardsLaserBeamIfHasLOSServerRpc(turret.NetworkObjectId, laserPointerRaycast.GetHashCode());

                PrevUsedTurret = turret;
            }
            else
            {
                if (PrevUsedTurret == null) return;
                PluginNetworkingInstance.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                PrevUsedTurret = null;
            }
        }
    }
}
