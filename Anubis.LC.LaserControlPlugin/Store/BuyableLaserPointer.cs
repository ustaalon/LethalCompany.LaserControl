using LethalLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Anubis.LC.LaserControlPlugin.Components;

namespace Anubis.LC.LaserControlPlugin.Store
{
    public static class BuyableLaserPointer
    {
        private static readonly int Price = 50;
        private static readonly string Name = "Pointer";
        private static readonly string Description = "To point laser to some direction";

        public static void RegisterShopItem()
        {
            List<Item> allItems =
            [
                .. Resources.FindObjectsOfTypeAll<Item>(),
                .. Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID),
            ];
            Item flashLaserPointer = allItems.FirstOrDefault(item => item.name == "FlashLaserPointer");

            if (flashLaserPointer != null)
            {
                Items.RegisterShopItem(CreateBuyableItem(flashLaserPointer), null, null, CreateInfoNode(Name, Description), Price);
            }
        }

        private static Item CreateBuyableItem(Item original)
        {
            Item item = Object.Instantiate<Item>(original);
            item.name = "Buyable" + original.name;
            item.isScrap = false;
            item.itemName = Name;
            item.creditsWorth = Price;
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
