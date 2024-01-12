using Anubis.LC.LaserControlPlugin.Components;
using HarmonyLib;
namespace Anubis.LC.LaserControlPlugin.Patches
{
    [HarmonyPatch(typeof(Turret))]
    public static class TurretPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start(Turret __instance)
        {
            __instance.gameObject.AddComponent<LaserPointerTargetTurret>();
        }
    }
}
