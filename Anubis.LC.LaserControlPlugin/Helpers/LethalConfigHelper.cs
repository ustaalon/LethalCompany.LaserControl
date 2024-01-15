using BepInEx.Configuration;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Helpers
{
    public static class LethalConfigHelper
    {
        public static ConfigEntry<bool> IsPointerBuyable;
        public static ConfigEntry<bool> IsPointerCanTurnOnAndOffTurrets;
        public static ConfigEntry<bool> IsBeta;

        public static void SetLehalConfig(ConfigFile config)
        {
            IsPointerBuyable = config.Bind("General", "Pointer Laser Buyable?", true, "The pointer laser is buyable (no scrap worth)");
            IsPointerCanTurnOnAndOffTurrets = config.Bind("General", "Can Turn On/Off Turrets?", true, "The laser pointer can turn off and on turrets");
            IsBeta = config.Bind("General", "Use Experimental Settings", false, "Use new features and logics");

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerBuyable, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerCanTurnOnAndOffTurrets, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsBeta, false));

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
