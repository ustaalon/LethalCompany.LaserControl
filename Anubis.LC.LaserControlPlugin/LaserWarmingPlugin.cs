using BepInEx;
using HarmonyLib;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using RuntimeNetcodeRPCValidator;

namespace Anubis.LC.LaserControlPlugin
{
    [BepInPlugin(ModStaticHelper.modGUID, ModStaticHelper.modName, ModStaticHelper.modVersion)]
    [BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
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

            LethalConfigHelper.SetLehalConfig(Config);

            LaserLogger.LogInfo($"{ModStaticHelper.modGUID} is loading...", true);

            LaserLogger.LogInfo($"Installing patches", true);
            HarmonyInstance.PatchAll(typeof(LaserControlPlugin).Assembly);


            DontDestroyOnLoad(this);

            LaserLogger.LogInfo($"Plugin {ModStaticHelper.modGUID} is loaded!", true);
        }
    }
}
