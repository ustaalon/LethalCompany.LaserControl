using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Extensions
{
    public static class TurretExtensions
    {
        public static void TurnTowardsLaserBeamIfHasLOS(this Turret turret, LaserPointerRaycast laserBeamObject)
        {
            if (laserBeamObject == null || laserBeamObject?.light == null) return;

            // Get the direction of the light beam
            Vector3 lightDirection = laserBeamObject.light.transform.forward;

            // Assuming the light is located at the position of the GameObject
            Vector3 lightPosition = laserBeamObject.light.transform.position;

            // Assuming a distance of 10 units for the light beam
            float beamDistance = 25f;

            // Calculate the position where the light beam ends
            Vector3 endPosition = lightPosition + lightDirection * beamDistance;

            if (Vector3.Distance(turret.transform.position, endPosition) <= beamDistance)
            {
                if (turret.turretActive == true && laserBeamObject.state)
                {
                    PluginNetworkingInstance.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Firing);
                    turret.tempTransform.position = endPosition;
                    turret.turnTowardsObjectCompass.LookAt(turret.tempTransform);
                }
                else
                {
                    PluginNetworkingInstance.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
                    PluginNetworkingInstance.Instance.StopTurretFireVisualServerRpc(turret.NetworkObjectId);
                    turret.turretMode = TurretMode.Detection;
                }
            }
            else
            {
                PluginNetworkingInstance.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
            }
        }
    }
}
