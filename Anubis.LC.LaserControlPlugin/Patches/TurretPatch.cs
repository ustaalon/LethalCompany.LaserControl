using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
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
            ModStaticHelper.Logger.LogInfo("Added LaserPointerTargetTurret to the turret");
            __instance.gameObject.AddComponent<LaserPointerTargetTurret>();
        }
    }
}
