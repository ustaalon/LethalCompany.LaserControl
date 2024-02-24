using BepInEx.Configuration;
using LethalConfig.ConfigItems;
using LethalConfig;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using LethalConfig.ConfigItems.Options;
using System.Diagnostics;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class LethalConfigHelper
    {
        public static Dictionary<string, object> HostConfigurationForPlayers = new Dictionary<string, object>();

        public static ConfigEntry<bool> IsPointerBuyable;
        public static ConfigEntry<bool> IsPointerCanTurnOnAndOffTurrets;
        public static ConfigEntry<bool> IsPointerCanDetonateLandmines;
        public static ConfigEntry<bool> IsPointerCanControlTurrets;
        public static ConfigEntry<float> PointerLaserDrainSpeed;
        public static ConfigEntry<int> PointerLaserPrice;
        public static ConfigEntry<bool> IsDebug;

        public static void SetLehalConfig(ConfigFile config)
        {
            IsDebug = config.Bind("Debug", "See all logs", true, "All logs will be throw to console");

            IsPointerBuyable = config.Bind("General", "Pointer Laser Buyable?", true, "The pointer laser is buyable (no scrap worth)");
            IsPointerBuyable.SettingChanged += (obj, args) =>
            {
                HostConfigurationForPlayers.Remove(nameof(IsPointerBuyable));
                HostConfigurationForPlayers.TryAdd(nameof(IsPointerBuyable), IsPointerBuyable.Value);
                if(Networking.Instance != null)
                {
                    Networking.Instance.SyncHostConfigurationServerRpc();
                }
            };

            PointerLaserPrice = config.Bind("General", "Laser Pointer Price", 50, "Determines laser pointer price");
            PointerLaserPrice.SettingChanged += (obj, args) =>
            {
                HostConfigurationForPlayers.Remove(nameof(PointerLaserPrice));
                HostConfigurationForPlayers.TryAdd(nameof(PointerLaserPrice), PointerLaserPrice.Value);
                if (Networking.Instance != null)
                {
                    Networking.Instance.SyncHostConfigurationServerRpc();
                }
            };

            IsPointerCanTurnOnAndOffTurrets = config.Bind("General", "Can Turn On/Off Turrets?", true, "The laser pointer can turn off and on turrets");
            IsPointerCanTurnOnAndOffTurrets.SettingChanged += (obj, args) =>
            {
                HostConfigurationForPlayers.Remove(nameof(IsPointerCanTurnOnAndOffTurrets));
                HostConfigurationForPlayers.TryAdd(nameof(IsPointerCanTurnOnAndOffTurrets), IsPointerCanTurnOnAndOffTurrets.Value);
                if (Networking.Instance != null)
                {
                    Networking.Instance.SyncHostConfigurationServerRpc();
                }
            };

            IsPointerCanDetonateLandmines = config.Bind("General", "Can Detonate Landmines?", true, "The laser pointer can detonate landmines");
            IsPointerCanDetonateLandmines.SettingChanged += (obj, args) =>
            {
                HostConfigurationForPlayers.Remove(nameof(IsPointerCanDetonateLandmines));
                HostConfigurationForPlayers.TryAdd(nameof(IsPointerCanDetonateLandmines), IsPointerCanDetonateLandmines.Value);
                if (Networking.Instance != null)
                {
                    Networking.Instance.SyncHostConfigurationServerRpc();
                }
            };

            IsPointerCanControlTurrets = config.Bind("General", "Can Control Turrets?", true, "The laser pointer can control turrets");
            IsPointerCanControlTurrets.SettingChanged += (obj, args) =>
            {
                HostConfigurationForPlayers.Remove(nameof(IsPointerCanControlTurrets));
                HostConfigurationForPlayers.TryAdd(nameof(IsPointerCanControlTurrets), IsPointerCanControlTurrets.Value);
                if (Networking.Instance != null)
                {
                    Networking.Instance.SyncHostConfigurationServerRpc();
                }
            };

            PointerLaserDrainSpeed = config.Bind("General", "Laser Pointer Drain Speed", 35f, "Determines the batery drain speed");
            PointerLaserDrainSpeed.SettingChanged += (obj, args) =>
            {
                HostConfigurationForPlayers.Remove(nameof(PointerLaserDrainSpeed));
                HostConfigurationForPlayers.TryAdd(nameof(PointerLaserDrainSpeed), PointerLaserDrainSpeed.Value);
                if (Networking.Instance != null)
                {
                    Networking.Instance.SyncHostConfigurationServerRpc();
                }
            };

            if (!HostConfigurationForPlayers.TryAdd(nameof(IsPointerBuyable), IsPointerBuyable.Value)
            || !HostConfigurationForPlayers.TryAdd(nameof(IsPointerCanTurnOnAndOffTurrets), IsPointerCanTurnOnAndOffTurrets.Value)
            || !HostConfigurationForPlayers.TryAdd(nameof(IsPointerCanControlTurrets), IsPointerCanControlTurrets.Value)
            || !HostConfigurationForPlayers.TryAdd(nameof(IsPointerCanDetonateLandmines), IsPointerCanDetonateLandmines.Value)
            || !HostConfigurationForPlayers.TryAdd(nameof(PointerLaserDrainSpeed), PointerLaserDrainSpeed.Value))
            {
                LaserLogger.LogError("Could not add mod configuration to dictionary");
            }

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerBuyable, true));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerCanTurnOnAndOffTurrets, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerCanDetonateLandmines, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerCanControlTurrets, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsDebug, false));
            LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(PointerLaserDrainSpeed, new FloatSliderOptions()
            {
                Min = 15f,
                Max = 100,
                RequiresRestart = false
            }));
            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(PointerLaserPrice, new IntSliderOptions()
            {
                Min = 15,
                Max = 3000,
                RequiresRestart = true
            }));

            LethalConfigManager.SetModIcon(LoadNewSprite(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png")));
            LethalConfigManager.SetModDescription("Binding the laser pointer to the turret. What bad things could happen?");
        }

        private static Sprite LoadNewSprite(string filePath, float pixelsPerUnit = 100.0f)
        {
            try
            {
                Texture2D spriteTexture = LoadTexture(filePath);
                var newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);

                return newSprite;
            }
            catch
            {
                return null;
            }
        }

        private static Texture2D LoadTexture(string filePath)
        {
            Texture2D tex2D;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex2D = new Texture2D(2, 2);
                if (tex2D.LoadImage(fileData))
                {
                    return tex2D;
                }
            }
            return null;
        }
    }
}
