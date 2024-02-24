using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using Anubis.LC.LaserControlPlugin.Store;
using HarmonyLib;
using System.Linq;

namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake(StartOfRound __instance)
        {
            var item = StartOfRound.Instance.allItemsList?.itemsList?.FirstOrDefault(itm => itm.spawnPrefab && itm.spawnPrefab.name == "LaserPointer");
            if (item != null)
            {
                LaserLogger.LogInfo("Added LaserPointerRaycastTarget to bind turrets and/or landmines and laser pointer");
                item.spawnPrefab.AddComponent<LaserPointerRaycastTarget>();

                var laserPointer = item.spawnPrefab.gameObject.GetComponent<FlashlightItem>();
                var laserPointerToolTips = laserPointer.itemProperties.toolTips.ToList();
                laserPointerToolTips.Add("Toggle turret shooting : [RMB]");
                laserPointer.itemProperties.toolTips = laserPointerToolTips.ToArray();

                LaserLogger.LogInfo("Added laser pointer to the ship's store");
                BuyableLaserPointer.RegisterShopItem(item.spawnPrefab, Networking.Instance.GetConfigItemValueOfPlayer<int>(nameof(LethalConfigHelper.PointerLaserPrice)));
            }
        }
    }
}
