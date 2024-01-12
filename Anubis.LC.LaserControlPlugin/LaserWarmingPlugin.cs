﻿using BepInEx;
using HarmonyLib;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using RuntimeNetcodeRPCValidator;

namespace Anubis.LC.LaserControlPlugin
{
    [BepInPlugin(ModStaticHelper.modGUID, ModStaticHelper.modName, ModStaticHelper.modVersion)]
    [BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
    public class LaserWarmingPlugin : BaseUnityPlugin
    {
        private Harmony HarmonyInstance = new Harmony(ModStaticHelper.modGUID);
        public static LaserWarmingPlugin? Instance;

        private NetcodeValidator netcodeValidator;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            netcodeValidator = new NetcodeValidator(ModStaticHelper.modGUID);
            netcodeValidator.PatchAll();

            netcodeValidator.BindToPreExistingObjectByBehaviour<PluginNetworkingInstance, Turret>();

            ModStaticHelper.Logger.LogInfo($"{ModStaticHelper.modGUID} is loading...");

            ModStaticHelper.Logger.LogInfo($"Installing patches");
            HarmonyInstance.PatchAll(typeof(LaserWarmingPlugin).Assembly);

            DontDestroyOnLoad(this);

            ModStaticHelper.Logger.LogInfo($"Plugin {ModStaticHelper.modGUID} is loaded!");
        }
    }
}
