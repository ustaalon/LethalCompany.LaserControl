using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using System.Collections;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class TurretExtensions
    {
        // Assuming a distance of 10 units for the light beam
        private static readonly float beamDistance = 35f;
        static Turret? PrevUsedTurret;

        public static IEnumerator TurnOffAndOnTurret(this Turret turret)
        {
            ModStaticHelper.Logger.LogError("-----------------");
            ModStaticHelper.Logger.LogInfo($"Turret Temp High -> OFF");
            ModStaticHelper.Logger.LogError("-----------------");
            Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
            turret.ToggleTurretEnabled(false);
            yield return new WaitForSeconds(3);
            turret.ToggleTurretEnabled(true);
            ModStaticHelper.Logger.LogError("-----------------");
            ModStaticHelper.Logger.LogInfo($"Turret Temp High -> ON");
            ModStaticHelper.Logger.LogError("-----------------");
        }

        private static Vector3 GetEndPositionOfBeam(Light light)
        {
            // Get the direction of the light beam
            Vector3 lightDirection = light.transform.forward;

            // Assuming the light is located at the position of the GameObject
            Vector3 lightPosition = light.transform.position;

            // Calculate the position where the light beam ends
            Vector3 endPosition = lightPosition + lightDirection * beamDistance;

            return endPosition;
        }

        public static void TurnTowardsLaserBeamIfHasLOS(this Turret turret, LaserPointerRaycast laserBeamObject)
        {
            if (laserBeamObject == null || laserBeamObject?.light == null) return;

            if (turret.turretActive == true && laserBeamObject.state)
            {
                if (PrevUsedTurret && PrevUsedTurret?.NetworkObjectId != turret.NetworkObjectId && PrevUsedTurret?.turretMode != TurretMode.Detection)
                {
                    ModStaticHelper.Logger.LogInfo("Previous turret no longer in player control, stop firing (ON)");
                    Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                }

                // Calculate the position where the light beam ends
                Vector3 endPosition = GetEndPositionOfBeam(laserBeamObject.light);

                ModStaticHelper.Logger.LogInfo("Turret firing by player control");
                Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Firing);
                turret.hasLineOfSight = true;
                turret.lostLOSTimer = 0f;
                turret.targetingDeadPlayer = false;
                turret.targetTransform = laserBeamObject.light.transform;
                turret.tempTransform.position = endPosition;
                turret.turnTowardsObjectCompass.LookAt(turret.tempTransform);

                PrevUsedTurret = turret;
            }
            else
            {
                if (turret.turretMode != TurretMode.Detection)
                {
                    turret.hasLineOfSight = false;
                    turret.targetTransform = null;
                    turret.lostLOSTimer = 0f;
                    ModStaticHelper.Logger.LogInfo("Turret no longer in player control, stop firing");
                    Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                }

                if (!laserBeamObject.state && PrevUsedTurret != null)
                {
                    if (PrevUsedTurret.turretMode != TurretMode.Detection)
                    {
                        ModStaticHelper.Logger.LogInfo("Previous turret no longer in player control, stop firing (OFF)");
                        Networking.Instance.SwitchTurretModeServerRpc(PrevUsedTurret.NetworkObjectId, TurretMode.Detection);
                    }
                    PrevUsedTurret = null;
                }
            }
        }
    }
}
