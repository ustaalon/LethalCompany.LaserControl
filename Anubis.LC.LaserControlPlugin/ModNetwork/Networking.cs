using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anubis.LC.LaserControlPlugin.ModNetwork
{
    internal class Networking : NetworkBehaviour
    {
        public static Networking Instance;

        private Turret[] currentTurrets = new Turret[0];
        private Dictionary<ulong, Turret> currentTurretsAsDict = new Dictionary<ulong, Turret>();

        private LaserPointerRaycast[] currentLaserPointerRaycast = new LaserPointerRaycast[0];
        private Dictionary<int, LaserPointerRaycast> currentLaserPointerRaycastAsDict = new Dictionary<int, LaserPointerRaycast>();

        private readonly float maxDistance = 35f;
        private readonly float minDistance = 20f;

        public Dictionary<int, LaserPointerRaycast> GetAllLaserPointerRaycastsAsDict()
        {
            return currentLaserPointerRaycastAsDict;
        }

        public Turret[] GetAllTurrets()
        {
            return currentTurrets;
        }

        public Dictionary<ulong, Turret> GetAllTurretsAsDict()
        {
            return currentTurretsAsDict;
        }

        private LaserPointerRaycast[] FindAllLaserPointerRaycasts()
        {
            LaserPointerRaycast[] laserPointerRaycasts = Object.FindObjectsOfType<LaserPointerRaycast>();

            if (laserPointerRaycasts.Count() == 0)
            {
                return new LaserPointerRaycast[0];
            }

            return laserPointerRaycasts;
        }

        private Turret[] FindAllTurrets()
        {
            Turret[] turretArray = Object.FindObjectsOfType<Turret>();

            if (turretArray.Count() == 0)
            {
                return new Turret[0];
            }

            return turretArray;
        }

        public Turret GetNearestTurret()
        {
            Transform localPlayerTransform = StartOfRound.Instance.localPlayerController.transform;
            Turret[] turretArray = GetAllTurrets();
            //Vector3 forward = Quaternion.Euler(0f, maxDistance, 0f) * localPlayerTransform.forward;

            // Check for turret in LOS
            //foreach (Turret turret in turretArray)
            //{
            //    if (Vector3.Distance(turret.transform.position, localPlayerTransform.position) > maxDistance) continue;

            //    Ray shootRay = new Ray(localPlayerTransform.position, forward);

            //    if (Physics.Raycast(shootRay, out RaycastHit hit, maxDistance, 1051400, QueryTriggerInteraction.Ignore))
            //    {
            //        if (hit.transform.CompareTag(turretArray[0].tag))
            //        {
            //            return turret;
            //        }
            //    }
            //}

            // If turret not in LOS, just find the closest one
            return turretArray
            .Where(turret => Vector3.Distance(turret.transform.position, localPlayerTransform.position) <= minDistance)
            .OrderBy(turret => Vector3.Distance(turret.transform.position, localPlayerTransform.position))
            .FirstOrDefault();
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopTurretFireVisualServerRpc(ulong networkObjectId)
        {
            if (!currentTurretsAsDict.ContainsKey(networkObjectId)) return;

            StopTurretFireVisualClientRpc(networkObjectId);
        }

        [ClientRpc]
        public void StopTurretFireVisualClientRpc(ulong networkObjectId)
        {
            if (GetAllTurretsAsDict().TryGetValue(networkObjectId, out Turret turret))
            {
                turret.mainAudio.Stop();
                turret.farAudio.Stop();
                turret.berserkAudio.Stop();
                turret.bulletParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
                turret.turretAnimator.SetInteger("TurretMode", 0);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SwitchTurretModeServerRpc(ulong networkObjectId, TurretMode turretMode)
        {
            if (!currentTurretsAsDict.ContainsKey(networkObjectId)) return;
            StopTurretFireVisualServerRpc(networkObjectId);
            SwitchTurretModeClientRpc(networkObjectId, turretMode);
        }

        [ClientRpc]
        public void SwitchTurretModeClientRpc(ulong networkObjectId, TurretMode turretMode)
        {
            if (GetAllTurretsAsDict().TryGetValue(networkObjectId, out Turret turret))
            {
                turret.turretMode = turretMode;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnTowardsLaserBeamIfHasLOSServerRpc(ulong networkObjectId, int laserPointerRaycastId)
        {
            if (GetAllTurretsAsDict().TryGetValue(networkObjectId, out Turret turret) &&
                GetAllLaserPointerRaycastsAsDict().TryGetValue(laserPointerRaycastId, out LaserPointerRaycast laserPointerRaycast))
            {
                TurnTowardsLaserBeamIfHasLOSClientRpc(networkObjectId, laserPointerRaycastId);
            }
        }

        [ClientRpc]
        public void TurnTowardsLaserBeamIfHasLOSClientRpc(ulong networkObjectId, int laserPointerRaycastId)
        {
            if (GetAllTurretsAsDict().TryGetValue(networkObjectId, out Turret turret) &&
                GetAllLaserPointerRaycastsAsDict().TryGetValue(laserPointerRaycastId, out LaserPointerRaycast laserPointerRaycast))
            {
                turret.TurnTowardsLaserBeamIfHasLOS(laserPointerRaycast);
            }
        }

        [ServerRpc]
        public void SyncAllTurretsAndRaycastsServerRpc()
        {
            SyncAllTurretsAndRaycastsClientRpc();
        }

        [ClientRpc]
        public void SyncAllTurretsAndRaycastsClientRpc()
        {
            ModStaticHelper.Logger.LogWarning("---- START SYNC TURRET ----");
            Turret[] turrets = FindAllTurrets();
            currentTurrets = turrets;
            currentTurretsAsDict = turrets.ToDictionary(t => t.NetworkObjectId);
            ModStaticHelper.Logger.LogWarning("---- END SYNC TURRET ----");

            ModStaticHelper.Logger.LogWarning("---- START SYNC RAYCAST ----");
            LaserPointerRaycast[] laserPointerRaycasts = FindAllLaserPointerRaycasts();
            currentLaserPointerRaycast = laserPointerRaycasts;
            currentLaserPointerRaycastAsDict = laserPointerRaycasts.ToDictionary(t => t.GetHashCode());
            ModStaticHelper.Logger.LogWarning("---- END SYNC RAYCAST ----");
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (!IsHost) // Any clients should ask for sync of something :shrug:
                    StartCoroutine(WaitForSomeTime());
            }
        }

        private IEnumerator WaitForSomeTime()
        {
            // We need to wait because sending an RPC before a NetworkObject is spawned results in errors.
            yield return new WaitUntil(() => NetworkObject.IsSpawned);

            // Tell all clients to run this method.
            SyncAllTurretsAndRaycastsServerRpc();
        }
    }
}
