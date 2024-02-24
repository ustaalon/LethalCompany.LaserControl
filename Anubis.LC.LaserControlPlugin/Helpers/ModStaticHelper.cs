using BepInEx.Bootstrap;
using BepInEx.Logging;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class ModStaticHelper
    {
        public const string modGUID = "Anubis.LaserControl";
        public const string modName = "LaserControl";
        public const string modVersion = "0.1.6";

        public static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        public static bool IsThisModInstalled(string mod)
        {
            if (Chainloader.PluginInfos.TryGetValue(mod, out BepInEx.PluginInfo modInfo))
            {
                Logger.LogInfo($"Mod {mod} is loaded alongside {modGUID}");
                return true;
            }
            return false;
        }
    }
}
