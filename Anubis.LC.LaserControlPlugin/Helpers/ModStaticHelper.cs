using BepInEx.Logging;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class ModStaticHelper
    {
        public const string modGUID = "Anubis.LaserControl";
        public const string modName = "LaserControl";
        public const string modVersion = "0.1.1";

        public static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
    }
}
