using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class TurretExtensions
    {
        // Assuming a distance of 10 units for the light beam
        private static readonly float beamDistance = 35f;

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

            // Calculate the position where the light beam ends
            Vector3 endPosition = GetEndPositionOfBeam(laserBeamObject.light);

            if (turret.turretActive == true && laserBeamObject.state && Vector3.Distance(turret.transform.position, endPosition) <= beamDistance)
            {
                ModStaticHelper.Logger.LogInfo("Turret firing by player control");
                Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Firing);
                turret.targetTransform = laserBeamObject.light.transform;
                turret.tempTransform.position = endPosition;
                turret.turnTowardsObjectCompass.LookAt(turret.tempTransform);
            }
            else
            {
                if (turret.turretMode != TurretMode.Detection)
                {
                    turret.targetTransform = null;
                    ModStaticHelper.Logger.LogInfo("Turret no longer in player control, stop firing");
                    Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                }
            }
        }
    }
}
