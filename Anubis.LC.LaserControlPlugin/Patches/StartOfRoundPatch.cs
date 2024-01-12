using Anubis.LC.LaserControlPlugin.Components;
using HarmonyLib;
using System.Collections.Generic;

namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake(StartOfRound __instance)
        {
            AllItemsList allItemsList = __instance.allItemsList;
            if (allItemsList == null)
            {
                return;
            }

            List<Item> itemsList = allItemsList.itemsList;
            if (itemsList != null)
            {
                Item item = itemsList.Find(itm => itm.spawnPrefab && itm.spawnPrefab.name == "LaserPointer");
                if (item == null || item.spawnPrefab == null)
                {
                    return;
                }
                item.spawnPrefab.AddComponent<LaserPointerRaycast>();
            }
        }
    }
}
