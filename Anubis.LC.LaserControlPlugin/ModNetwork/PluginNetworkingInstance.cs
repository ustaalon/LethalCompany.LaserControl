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
    internal class PluginNetworkingInstance : NetworkBehaviour
    {
        public static PluginNetworkingInstance Instance;

        private Turret[] currentTurrets = new Turret[0];
        private Dictionary<ulong, Turret> currentTurretsAsDict = new Dictionary<ulong, Turret>();

        private LaserPointerRaycast[] currentLaserPointerRaycast = new LaserPointerRaycast[0];
        private Dictionary<int, LaserPointerRaycast> currentLaserPointerRaycastAsDict = new Dictionary<int, LaserPointerRaycast>();

        private readonly float minDistance = 25f;

        public LaserPointerRaycast[] GetLaserPointerRaycasts()
        {
            return currentLaserPointerRaycast;
        }

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

        public bool IsTurretExistsByNetObjId(ulong netObjId)
        {
            return GetAllTurrets().SingleOrDefault(t => t.NetworkObjectId == netObjId);
        }

        public LaserPointerRaycast[] FindAllLaserPointerRaycasts()
        {
            LaserPointerRaycast[] laserPointerRaycasts = Object.FindObjectsOfType<LaserPointerRaycast>();

            if (laserPointerRaycasts.Count() == 0)
            {
                return new LaserPointerRaycast[0];
            }

            return laserPointerRaycasts;
        }

        public Turret[] FindAllTurrets()
        {
            Turret[] turretArray = Object.FindObjectsOfType<Turret>();

            if (turretArray.Count() == 0)
            {
                return new Turret[0];
            }

            return turretArray;
        }

        public Turret GetNearestTurret(Transform localPlayerTransform)
        {
            Turret[] turretArray = GetAllTurrets();

            return turretArray
                .Where(turret => Vector3.Distance(turret.transform.position, localPlayerTransform.position) <= minDistance)
                .OrderBy(turret => Vector3.Distance(turret.transform.position, localPlayerTransform.position))
                .FirstOrDefault();
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopTurretFireVisualServerRpc(ulong networkObjectId)
        {
            if (!IsTurretExistsByNetObjId(networkObjectId)) return;

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
            if (!IsTurretExistsByNetObjId(networkObjectId)) return;

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
            if (!IsTurretExistsByNetObjId(networkObjectId)) return;

            TurnTowardsLaserBeamIfHasLOSClientRpc(networkObjectId, laserPointerRaycastId);
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
        public void SyncAllTurretsServerRpc()
        {
            SyncAllTurretsClientRpc();
        }

        [ClientRpc]
        public void SyncAllTurretsClientRpc()
        {
            ModStaticHelper.Logger.LogWarning("---- START SYNC TURRET ----");
            Turret[] turrets = FindAllTurrets();
            currentTurrets = turrets;
            currentTurretsAsDict = turrets.ToDictionary(t => t.NetworkObjectId);
            ModStaticHelper.Logger.LogWarning("---- END SYNC TURRET ----");


            ModStaticHelper.Logger.LogWarning("---- START SYNC RAYCAST ----");
            LaserPointerRaycast[] laserPointerRaycasts = FindAllLaserPointerRaycasts();
            currentLaserPointerRaycast = laserPointerRaycasts;
            foreach (LaserPointerRaycast laser in laserPointerRaycasts)
            {
                ModStaticHelper.Logger.LogInfo($"{laser.GetHashCode()}, {laser.GetInstanceID()}");
            }
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
            SyncAllTurretsServerRpc();
        }
    }
}
