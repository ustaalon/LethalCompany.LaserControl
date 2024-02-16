using BepInEx.Bootstrap;
using BepInEx.Logging;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class LaserLogger
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ModStaticHelper.modGUID);

        internal static void Log(LogLevel level, object data, bool onlyWhenDebug = true)
        {
            if (onlyWhenDebug && level != LogLevel.Error && !LethalConfigHelper.IsDebug.Value) return;
            Logger.Log(level, data);
        }

        internal static void LogFatal(object data, bool onlyWhenDebug = true)
        {
            Log(LogLevel.Fatal, data, onlyWhenDebug);
        }

        internal static void LogError(object data, bool onlyWhenDebug = true)
        {
            Log(LogLevel.Error, data, onlyWhenDebug);
        }

        internal static void LogWarning(object data, bool onlyWhenDebug = true)
        {
            Log(LogLevel.Warning, data, onlyWhenDebug);
        }

        internal static void LogMessage(object data, bool onlyWhenDebug = true)
        {
            Log(LogLevel.Message, data, onlyWhenDebug);
        }

        internal static void LogInfo(object data, bool onlyWhenDebug = true)
        {
            Log(LogLevel.Info, data, onlyWhenDebug);
        }

        internal static void LogDebug(object data, bool onlyWhenDebug = true)
        {
            Log(LogLevel.Debug, data, onlyWhenDebug);
        }
    }
}
