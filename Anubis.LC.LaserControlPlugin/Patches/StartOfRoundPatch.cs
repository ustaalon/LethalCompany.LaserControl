using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.Store;
using HarmonyLib;
using System.Collections.Generic;
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
            if (LethalConfigHelper.IsPointerCanTurnOnAndOffTurrets.Value)
            {
                var item = StartOfRound.Instance.allItemsList?.itemsList?.FirstOrDefault(itm => itm.spawnPrefab && itm.spawnPrefab.name == "LaserPointer");
                if (item != null)
                {
                    if (item.spawnPrefab.GetComponent<LaserPointerTurretOnAndOff>() == null)
                    {
                        ModStaticHelper.Logger.LogInfo("Added LaserPointerTurretOnAndOff to bind turret and laser pointer");
                        item.spawnPrefab.AddComponent<LaserPointerTurretOnAndOff>();
                    }
                }
            }

            if (LethalConfigHelper.IsPointerBuyable.Value)
            {
                ModStaticHelper.Logger.LogInfo("Added laser pointer to the ship's store");
                BuyableLaserPointer.RegisterShopItem();
            }
        }
    }
}
