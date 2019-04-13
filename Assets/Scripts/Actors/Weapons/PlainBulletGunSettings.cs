using System;

namespace GFun
{
    [Serializable]
    public class PlainBulletGunSettings
    {
        public int Level = 1;
        public int AmmoCount = 100;
        public int AmmoMax => 100;
        public FiringSpread FiringSpread = FiringSpread.Single;
        public FiringMode FiringMode = FiringMode.Single;
        public float Precision = 1.0f;
        public int BurstCount = 1;
        public float TimeBetweenShots = 500;
    }
}
