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
        public static ConfigEntry<bool> IsPointerPurchasable;
        public static ConfigEntry<bool> IsBeta;

        public static void SetLehalConfig(ConfigFile config)
        {
            IsPointerPurchasable = config.Bind("General", "Is Pointer Laser Purchasable?", true, "The pointer laser is also buyable (no scrap worth)");
            IsBeta = config.Bind("General", "Use Experimental Settings", false, "Use new features and logics");

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(IsPointerPurchasable, false));
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
