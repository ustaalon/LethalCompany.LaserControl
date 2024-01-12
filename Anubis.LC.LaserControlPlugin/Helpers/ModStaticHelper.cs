using BepInEx.Logging;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class ModStaticHelper
    {
        public const string modGUID = "Anubis.LaserWarming";
        public const string modName = "LaserWarming";
        public const string modVersion = "1.0.0";

        public static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
    }
}
