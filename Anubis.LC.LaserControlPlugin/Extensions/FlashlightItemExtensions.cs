using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class FlashlightItemExtensions
    {
        public static void UseLaserPointerItemBatteries(this FlashlightItem flashlightItem)
        {
            flashlightItem.insertedBattery.charge -= Time.deltaTime / 15f;
        }
    }
}
