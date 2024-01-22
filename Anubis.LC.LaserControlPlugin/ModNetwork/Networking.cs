using Anubis.LC.LaserControlPlugin.Components;
using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.Store;
using BepInEx.Configuration;
using LethalLib.Modules;
using RuntimeNetcodeRPCValidator;
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
        private readonly float minDistance = 25f;

        private Dictionary<string, bool> HostConfigurationForPlayers = new Dictionary<string, bool>();

        public bool GetConfigItemValueOfPlayer(string key)
        {
            if (!IsHost && HostConfigurationForPlayers.TryGetValue(key, out bool playerValue))
            {
                return playerValue;
            }

            if (IsHost && LethalConfigHelper.HostConfigurationForPlayers.TryGetValue(key, out bool hostValue))
            {
                return hostValue;
            }

            return false;
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
            StopTurretFireVisualClientRpc(networkObjectId);
        }

        [ClientRpc]
        public void StopTurretFireVisualClientRpc(ulong networkObjectId)
        {
            if (GetAllTurretsAsDict().TryGetValue(networkObjectId, out Turret turret))
            {
                if (turret.turretMode == TurretMode.Firing)
                {
                    turret.hasLineOfSight = false;
                    turret.lostLOSTimer = 0f;
                    turret.mainAudio.Stop();
                    turret.farAudio.Stop();
                    turret.berserkAudio.Stop();
                    turret.bulletCollisionAudio.Stop();
                    turret.bulletParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
                    turret.turretAnimator.SetInteger("TurretMode", 0);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SwitchTurretModeServerRpc(ulong networkObjectId, TurretMode turretMode)
        {
            if (turretMode == TurretMode.Detection)
            {
                StopTurretFireVisualServerRpc(networkObjectId);
            }
            SwitchTurretModeClientRpc(networkObjectId, turretMode);
        }

        [ClientRpc]
        public void SwitchTurretModeClientRpc(ulong networkObjectId, TurretMode turretMode)
        {
            if (GetAllTurretsAsDict().TryGetValue(networkObjectId, out Turret turret))
            {
                turret.turretMode = turretMode;
                turret.SwitchTurretMode((int)turretMode);
                turret.SetToModeClientRpc((int)turretMode);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnTowardsLaserBeamIfHasLOSServerRpc(ulong networkObjectId, int laserPointerRaycastId)
        {
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

        [ServerRpc(RequireOwnership = false)]
        public void SyncAllTurretsAndRaycastsServerRpc()
        {
            SyncAllTurretsAndRaycastsClientRpc();
        }

        [ClientRpc]
        public void SyncAllTurretsAndRaycastsClientRpc()
        {
            ModStaticHelper.Logger.LogInfo("Syncing turrets and laser pointers");
            Turret[] turrets = FindAllTurrets();
            currentTurrets = turrets;
            currentTurretsAsDict = turrets.ToDictionary(t => t.NetworkObjectId);
            LaserPointerRaycast[] laserPointerRaycasts = FindAllLaserPointerRaycasts();
            ModStaticHelper.Logger.LogError("---------------");
            foreach (var laser in laserPointerRaycasts)
            {
                ModStaticHelper.Logger.LogInfo($"Laser pointer network Id ${laser.GetHashCode()}");
            }
            ModStaticHelper.Logger.LogError("---------------");
            currentLaserPointerRaycast = laserPointerRaycasts;
            currentLaserPointerRaycastAsDict = laserPointerRaycasts.ToDictionary(t => t.GetHashCode());
        }

        [ServerRpc]
        public void SyncHostConfigurationServerRpc()
        {
            if (!IsHost) return;
            ModStaticHelper.Logger.LogInfo("Syncing host mod configuration for other players");
            SyncHostConfigurationClientRpc(nameof(LethalConfigHelper.IsPointerBuyable), LethalConfigHelper.IsPointerBuyable.Value);
            SyncHostConfigurationClientRpc(nameof(LethalConfigHelper.IsPointerCanTurnOnAndOffTurrets), LethalConfigHelper.IsPointerCanTurnOnAndOffTurrets.Value);
            SyncHostConfigurationClientRpc(nameof(LethalConfigHelper.IsPointerCanControlTurrets), LethalConfigHelper.IsPointerCanControlTurrets.Value);
            SyncHostConfigurationClientRpc(nameof(LethalConfigHelper.IsPointerCanDetonateLandmines), LethalConfigHelper.IsPointerCanDetonateLandmines.Value);
        }

        [ClientRpc]
        public void SyncHostConfigurationClientRpc(string key, bool value)
        {
            ModStaticHelper.Logger.LogInfo($"Syncing host mod configuration for player (key: {key}, value: {value})");
            HostConfigurationForPlayers.Remove(key);
            if (!HostConfigurationForPlayers.TryAdd(key, value))
            {
                ModStaticHelper.Logger.LogWarning($"Error while syncing host mod configuration for player (key: {key}, value: {value})");
            }

            if (key.Equals(nameof(LethalConfigHelper.IsPointerBuyable)) && HostConfigurationForPlayers.TryGetValue(nameof(LethalConfigHelper.IsPointerBuyable), out var isPointerBuyableValue) && !isPointerBuyableValue)
            {
                ModStaticHelper.Logger.LogInfo("Laser pointer removed from the ship's store");
                Items.RemoveShopItem(BuyableLaserPointer.LaserPointerItemInstance);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AskHostForSyncServerRpc()
        {
            ModStaticHelper.Logger.LogInfo("Asking host to sync configuration");
            SyncHostConfigurationServerRpc();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                StartCoroutine(WaitForSomeTime());
            }
        }

        private IEnumerator WaitForSomeTime()
        {
            // We need to wait because sending an RPC before a NetworkObject is spawned results in errors.
            yield return new WaitUntil(() => NetworkObject.IsSpawned);
            SyncAllTurretsAndRaycastsServerRpc();
            AskHostForSyncServerRpc();
        }
    }
}
