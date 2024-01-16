using Anubis.LC.LaserControlPlugin.Helpers;
using UnityEngine;

namespace Anubis.LC.LaserControlPlugin.Components
{
    public class LaserPointerRaycast : MonoBehaviour
    {
        private FlashlightItem? item;

        public Light light
        {
            get
            {
                if (!item.usingPlayerHelmetLight || !item.playerHeldBy)
                {
                    return item.flashlightBulb;
                }
                return item.playerHeldBy.helmetLight;
            }
        }

        public bool state
        {
            get
            {
                if (item.isBeingUsed)
                {
                    return item.IsOwner && !item.usingPlayerHelmetLight;
                }
                return false;
            }
        }

        public void Awake()
        {
            item = GetComponent<FlashlightItem>();
        }
    }
}
