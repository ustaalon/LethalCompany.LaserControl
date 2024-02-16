using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using Anubis.LC.LaserControlPlugin.ModNetwork;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Components
{
    public class LaserPointerTargetLandmine : LaserPointerTarget
    {
        private readonly float radius = 0.53f;
        private readonly float maxTemp = 1f;
        protected Landmine? target;

        protected new void Awake()
        {
            base.Awake();
            target = GetComponent<Landmine>();
        }

        public override void Warmup(Transform origin, FlashlightItem laserPointer)
        {
            if (!Networking.Instance.GetConfigItemValueOfPlayer<bool>(nameof(LethalConfigHelper.IsPointerCanDetonateLandmines)) || !target || target.hasExploded) return;

            if (!triggered && HasCollision(origin.position, origin.forward, transform.position + offset, radius)
                 && !Physics.Linecast(origin.position, hitPoint, 1051400, QueryTriggerInteraction.Ignore))
            {
                laserPointer.UseLaserPointerItemBatteries();
                tempCounter += Time.deltaTime;

                if (tempCounter >= maxTemp)
                {
                    LaserLogger.LogDebug("Landmine detonated");
                    target.TriggerMineOnLocalClientByExiting();
                }
            }
        }
    }
}
