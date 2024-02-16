using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using HarmonyLib;
namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    public static class LandminePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start(Landmine __instance)
        {
            LaserLogger.LogDebug("Added LaserPointerTargetLandmine to the landmine");
            __instance.gameObject.AddComponent<LaserPointerTargetLandmine>();
        }
    }
}
