using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using System.Collections;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Components
{
    public class LaserPointerTargetTurret : LaserPointerTarget
    {
        private readonly float radius = 1f;
        private readonly float maxTemp = 0.3f;
        private bool IsStartedCorouting = false;

        protected Turret? target;

        protected new void Awake()
        {
            base.Awake();
            target = GetComponent<Turret>();
        }

        public override void Warmup(Transform origin, FlashlightItem laserPointer)
        {
            if (!LethalConfigHelper.IsPointerCanTurnOnAndOffTurrets.Value) return;
            if (!target) return;

            if (!triggered && HasCollision(origin.position, origin.forward, transform.position + offset, radius)
                 && !Physics.Linecast(origin.position, hitPoint, 1051400, QueryTriggerInteraction.Ignore))
            {
                laserPointer.UseLaserPointerItemBatteries();
                tempCounter += Time.deltaTime;

                if (tempCounter >= maxTemp && !IsStartedCorouting)
                {
                    StartCoroutine(TurnOffAndOnTurret(target));
                }
            }
        }

        private IEnumerator TurnOffAndOnTurret(Turret turret)
        {
            ModStaticHelper.Logger.LogError("-----------------");
            ModStaticHelper.Logger.LogInfo($"Turret Temp High -> OFF");
            ModStaticHelper.Logger.LogError("-----------------");
            IsStartedCorouting = true;
            triggered = true;
            turret.ToggleTurretEnabled(false);
            Networking.Instance.SwitchTurretModeServerRpc(turret.NetworkObjectId, TurretMode.Detection);
            yield return new WaitForSeconds(3);
            turret.ToggleTurretEnabled(true);
            ModStaticHelper.Logger.LogError("-----------------");
            ModStaticHelper.Logger.LogInfo($"Turret Temp High -> ON");
            ModStaticHelper.Logger.LogError("-----------------");
            tempCounter = 0f;
            triggered = false;
            IsStartedCorouting = false;
        }
    }
}
