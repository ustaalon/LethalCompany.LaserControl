using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class TurretExtensions
    {
        // Assuming a distance of 10 units for the light beam
        private static readonly float beamDistance = 25f;

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

            if (Vector3.Distance(turret.transform.position, endPosition) > beamDistance)
            {
                if (turret.turretMode != TurretMode.Detection)
                {
                    Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                }
                return;
            }

            if (turret.turretActive == true && laserBeamObject.state)
            {
                Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Firing);
                turret.tempTransform.position = endPosition;
                turret.turnTowardsObjectCompass.LookAt(turret.tempTransform);
            }
            else
            {
                if (turret.turretMode != TurretMode.Detection)
                {
                    Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                }
            }
        }
    }
}
