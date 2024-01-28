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
        private readonly float maxTemp = 0.4f;
        private bool IsStartedCorouting = false;

        protected Turret? target;

        protected new void Awake()
        {
            base.Awake();
            offset = Vector3.up * 0.2f;
            target = GetComponent<Turret>();
        }

        public override void Warmup(Transform origin, FlashlightItem laserPointer)
        {
            if (!Networking.Instance.GetConfigItemValueOfPlayer(nameof(LethalConfigHelper.IsPointerCanTurnOnAndOffTurrets)) || !target) return;

            if (!triggered && HasCollision(origin.position, origin.forward, transform.position + offset, radius)
                 && !Physics.Linecast(origin.position, hitPoint, 1051400, QueryTriggerInteraction.Ignore))
            {
                laserPointer.UseLaserPointerItemBatteries(target, 5);
                tempCounter += Time.deltaTime;

                if (tempCounter >= maxTemp && !IsStartedCorouting)
                {
                    StartCoroutine(TurnOffAndOnTurret(target));
                }
            }
        }

        private IEnumerator TurnOffAndOnTurret(Turret turret)
        {
            IsStartedCorouting = true;
            triggered = true;
            yield return turret.TurnOffAndOnTurret();
            tempCounter = 0f;
            triggered = false;
            IsStartedCorouting = false;
        }
    }
}
