using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class FlashlightItemExtensions
    {
        public static void UseLaserPointerItemBatteries(this FlashlightItem flashlightItem)
        {
            // next release
            //flashlightItem.insertedBattery.charge -= Time.deltaTime / 0.5f;
        }

        public static Turret? CheckForNearestTurret(this FlashlightItem flashlightItem)
        {
            StartOfRound startOfRound = Object.FindObjectOfType<StartOfRound>();
            Transform localPlayerTransform = startOfRound.localPlayerController.transform;

            return PluginNetworkingInstance.Instance.GetNearestTurret(localPlayerTransform);
        }
    }
}
