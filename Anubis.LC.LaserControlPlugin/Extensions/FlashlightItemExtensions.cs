using Anubis.LC.LaserControlPlugin.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class FlashlightItemExtensions
    {
        public static LaserPointerRaycast LaserPointerRaycastCurrentInstance;

        public static void UseLaserPointerItemBatteries(this FlashlightItem flashlightItem)
        {
            //flashlightItem.insertedBattery.charge -= Time.deltaTime / 40f;
            flashlightItem.DestroyIfBatteryIsEmpty();
        }

        public static bool DestroyIfBatteryIsEmpty(this FlashlightItem flashlightItem)
        {
            if (LaserPointerRaycastCurrentInstance && flashlightItem.insertedBattery.charge <= 0f)
            {
                Object.Destroy(LaserPointerRaycastCurrentInstance);
                return true;
            }
            return false;
        }
    }
}
