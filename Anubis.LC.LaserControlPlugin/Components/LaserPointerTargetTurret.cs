using Anubis.LC.LaserControlPlugin.Extensions;
using Anubis.LC.LaserControlPlugin.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Components
{
    public class LaserPointerTargetTurret : MonoBehaviour
    {
        public static List<LaserPointerTargetTurret> Instances = new List<LaserPointerTargetTurret>();
        public static int Count => Instances.Count;

        private readonly float radius = 1f;
        private readonly float maxTemp = 0.3f;
        private readonly float timerMaxCounting = 3f;

        private float tempCounter;

        private bool IsStartedCorouting = false;
        private bool triggered = false;

        private Turret? turret;

        private Vector3 offset = Vector3.up * 0.1f;
        private Vector3 hitPoint;

        public void Awake()
        {
            turret = GetComponent<Turret>();
            Instances.Add(this);
        }

        public void OnDestroy()
        {
            Instances.Remove(this);
        }

        public void ToggleTurretByTemp(Transform origin, FlashlightItem laserPointer)
        {
            if (!turret) return;

            if (!triggered && HasCollision(origin.position, origin.forward, transform.position + offset, radius)
                 && !Physics.Linecast(origin.position, hitPoint, 1051400, QueryTriggerInteraction.Ignore))
            {
                laserPointer.UseLaserPointerItemBatteries();
                tempCounter += Time.deltaTime;

                if (tempCounter >= maxTemp && !IsStartedCorouting)
                {
                    StartCoroutine(TurnOffAndOnTurret(turret, timerMaxCounting));
                }
            }
        }

        private IEnumerator TurnOffAndOnTurret(Turret turret, float seconds = 5f)
        {
            ModStaticHelper.Logger.LogError("-----------------");
            ModStaticHelper.Logger.LogInfo($"Turret Temp High -> OFF");
            ModStaticHelper.Logger.LogError("-----------------");
            IsStartedCorouting = true;
            triggered = true;
            turret.ToggleTurretEnabled(false);
            turret.SetToModeClientRpc((int)TurretMode.Detection);
            yield return new WaitForSeconds(seconds);
            turret.ToggleTurretEnabled(true);
            ModStaticHelper.Logger.LogError("-----------------");
            ModStaticHelper.Logger.LogInfo($"Turret Temp High -> ON");
            ModStaticHelper.Logger.LogError("-----------------");
            tempCounter = 0f;
            triggered = false;
            IsStartedCorouting = false;
        }

        private bool HasCollision(Vector3 point, Vector3 direction, Vector3 center, float radius = 1f)
        {
            float num = Vector3.Dot(center - point, Vector3.up) / Vector3.Dot(direction, Vector3.up);
            hitPoint = point + direction * num;
            if (num > 0f)
            {
                return Vector3.Distance(center, hitPoint) <= radius;
            }
            return false;
        }
    }
}
