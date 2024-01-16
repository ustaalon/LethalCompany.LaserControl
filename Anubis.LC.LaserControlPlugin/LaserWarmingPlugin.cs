using BepInEx;
using HarmonyLib;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using RuntimeNetcodeRPCValidator;

namespace Anubis.LC.LaserControlPlugin
{
    [BepInPlugin(ModStaticHelper.modGUID, ModStaticHelper.modName, ModStaticHelper.modVersion)]
    [BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class LaserControlPlugin : BaseUnityPlugin
    {
        private Harmony HarmonyInstance = new Harmony(ModStaticHelper.modGUID);
        public static LaserControlPlugin? Instance;

        private NetcodeValidator netcodeValidator;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            netcodeValidator = new NetcodeValidator(ModStaticHelper.modGUID);
            netcodeValidator.PatchAll();

            netcodeValidator.BindToPreExistingObjectByBehaviour<Networking, Terminal>();

            ModStaticHelper.Logger.LogInfo($"{ModStaticHelper.modGUID} is loading...");

            ModStaticHelper.Logger.LogInfo($"Installing patches");
            HarmonyInstance.PatchAll(typeof(LaserControlPlugin).Assembly);

            LethalConfigHelper.SetLehalConfig(Config);

            DontDestroyOnLoad(this);

            ModStaticHelper.Logger.LogInfo($"Plugin {ModStaticHelper.modGUID} is loaded!");
        }
    }
}
