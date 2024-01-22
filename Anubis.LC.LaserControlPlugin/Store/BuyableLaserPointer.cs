using LethalLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Store
{
    public static class BuyableLaserPointer
    {
        private static readonly int Price = 50;
        private static readonly string Name = "Pointer";
        private static readonly string Description = "To point laser to some direction";

        public static Item LaserPointerItemInstance { get; private set; }

        public static void RegisterShopItem(GameObject spawnPrefab)
        {
            List<Item> allItems =
            [
                .. Resources.FindObjectsOfTypeAll<Item>(),
                .. Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID),
            ];
            Item flashLaserPointer = allItems.FirstOrDefault(item => item.name == "FlashLaserPointer");

            if (flashLaserPointer != null && !LaserPointerItemInstance)
            {
                Items.RegisterShopItem(CreateBuyableItem(flashLaserPointer, spawnPrefab), null, null, CreateInfoNode(Name, Description), Price);
            }
        }

        private static Item CreateBuyableItem(Item original, GameObject spawnPrefab)
        {
            Item item = Object.Instantiate<Item>(original);
            item.name = "Buyable" + original.name;
            item.isScrap = false;
            item.itemName = Name;
            item.creditsWorth = Price;
            item.spawnPrefab = spawnPrefab;
            LaserPointerItemInstance = item;
            return item;
        }

        private static TerminalNode CreateInfoNode(string name, string description)
        {
            TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
            terminalNode.clearPreviousText = true;
            terminalNode.name = name + "InfoNode";
            terminalNode.displayText = description + "\n\n";
            return terminalNode;
        }
    }
}
