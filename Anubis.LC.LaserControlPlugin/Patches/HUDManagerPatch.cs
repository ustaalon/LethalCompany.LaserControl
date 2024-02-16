using Anubis.LC.LaserControlPlugin.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("ChangeControlTipMultiple")]
        [HarmonyPostfix]
        public static void ChangeControlTipMultiple(HUDManager __instance, string[] allLines, bool holdingItem = false, Item itemProperties = null)
        {
            if (__instance.controlTipLines == null || __instance.controlTipLines.Length == 0) return;
            for (int i = 0; i < __instance.controlTipLines.Length; i++)
            {
                LaserLogger.LogDebug($"{__instance.controlTipLines[i].text}");
                if (!__instance.controlTipLines[i].text.Contains("Toggle turret shooting : [LMB]", StringComparison.OrdinalIgnoreCase)) continue;
                __instance.controlTipLines[i].text = "Toggle turret shooting : [RMB]";
            }
        }
    }
}
