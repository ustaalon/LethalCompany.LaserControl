using BepInEx.Bootstrap;
using BepInEx.Logging;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class LaserLogger
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ModStaticHelper.modGUID);

        internal static void Log(LogLevel level, object data, bool overrideDebugConfiguration = false)
        {
            if (overrideDebugConfiguration)
            {
                Logger.Log(level, data);
            }
            else
            {
                if (level != LogLevel.Error && !LethalConfigHelper.IsDebug.Value) return;
                Logger.Log(level, data);
            }
        }

        internal static void LogFatal(object data, bool overrideDebugConfiguration = false)
        {
            Log(LogLevel.Fatal, data, overrideDebugConfiguration);
        }

        internal static void LogError(object data, bool overrideDebugConfiguration = false)
        {
            Log(LogLevel.Error, data, overrideDebugConfiguration);
        }

        internal static void LogWarning(object data, bool overrideDebugConfiguration = false)
        {
            Log(LogLevel.Warning, data, overrideDebugConfiguration);
        }

        internal static void LogMessage(object data, bool overrideDebugConfiguration = false)
        {
            Log(LogLevel.Message, data, overrideDebugConfiguration);
        }

        internal static void LogInfo(object data, bool overrideDebugConfiguration = false)
        {
            Log(LogLevel.Info, data, overrideDebugConfiguration);
        }

        internal static void LogDebug(object data, bool overrideDebugConfiguration = false)
        {
            Log(LogLevel.Debug, data, overrideDebugConfiguration);
        }
    }
}
