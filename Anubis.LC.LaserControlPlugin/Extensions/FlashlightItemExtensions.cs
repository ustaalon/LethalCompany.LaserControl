using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class FlashlightItemExtensions
    {
        public static LaserPointerRaycast LaserPointerRaycastCurrentInstance;

        public static void UseLaserPointerItemBatteries(this FlashlightItem flashlightItem, Turret? turret = null, float reduceAmount = 35f)
        {
            flashlightItem.insertedBattery.charge -= Time.deltaTime / reduceAmount;
            flashlightItem.DestroyIfBatteryIsEmpty(turret);
        }

        public static IEnumerator DestroyIfBatteryIsEmpty(this FlashlightItem flashlightItem, Turret? turret = null)
        {
            if (flashlightItem.insertedBattery.charge <= 0f)
            {
                if (LaserPointerRaycastCurrentInstance)
                {
                    Object.Destroy(LaserPointerRaycastCurrentInstance);
                }

                if (turret)
                {
                    ModStaticHelper.Logger.LogInfo("No battery to the laser pointer. Turning off turret");
                    Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                    yield return turret.TurnOffAndOnTurret();
                }
                yield return true;
            }
            yield return false;
        }
    }
}
