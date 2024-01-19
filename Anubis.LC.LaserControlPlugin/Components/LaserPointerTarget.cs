using System.Collections.Generic;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Components
{
    public interface ILaserPointerTarget {
        void Warmup(Transform origin, FlashlightItem laserPointer);
    }

    public abstract class LaserPointerTarget : MonoBehaviour, ILaserPointerTarget
    {
        public static List<ILaserPointerTarget> Instances = new List<ILaserPointerTarget>();
        public static int Count => Instances.Count;

        protected float tempCounter;

        protected bool triggered = false;

        protected Vector3 offset = Vector3.up * 0.1f;
        protected Vector3 hitPoint;

        protected void Awake()
        {
            Instances.Add(this);
        }

        protected void OnDestroy()
        {
            Instances.Remove(this);
        }

        public abstract void Warmup(Transform origin, FlashlightItem laserPointer);

        protected bool HasCollision(Vector3 point, Vector3 direction, Vector3 center, float radius = 1f)
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
