using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class FlashlightItemExtensions
    {
        public static void UseLaserPointerItemBatteries(this FlashlightItem flashlightItem)
        {
            flashlightItem.insertedBattery.charge -= Time.deltaTime / 0.1f;
        }
    }
}
