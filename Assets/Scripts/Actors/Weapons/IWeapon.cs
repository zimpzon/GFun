using UnityEngine;

namespace GFun
{
    public class IWeapon : MonoBehaviour
    {
        public AmmoType AmmoType { get; }
        public int MaxAmmo { get; }
        public int AmmoCount { get; }

        public FiringMode FiringMode{ get; }
        public int BurstCount { get; }
        public float TimeBetweenBurstShots { get; }
        public float TimeBetweenAutoShots { get; }
    }
}
