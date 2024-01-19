﻿using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class FlashlightItemExtensions
    {
        public static LaserPointerRaycast LaserPointerRaycastCurrentInstance;

        public static void UseLaserPointerItemBatteries(this FlashlightItem flashlightItem)
        {
            flashlightItem.insertedBattery.charge -= Time.deltaTime / 35f;
            flashlightItem.DestroyIfBatteryIsEmpty();
        }

        public static IEnumerator DestroyIfBatteryIsEmpty(this FlashlightItem flashlightItem, Turret? turret = null)
        {
            if(turret && flashlightItem.insertedBattery.charge <= 0f)
            {
                ModStaticHelper.Logger.LogInfo("No battery to the laser pointer. Turning off turret");
                Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                yield return turret.TurnOffAndOnTurret();
            }

            if (LaserPointerRaycastCurrentInstance && flashlightItem.insertedBattery.charge <= 0f)
            {
                Object.Destroy(LaserPointerRaycastCurrentInstance);
                yield return true;
            }
            yield return false;
        }
    }
}
