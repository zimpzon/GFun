using System;

namespace GFun
{
    [Serializable]
    public class PlainBulletSettings
    {
        public float Speed = 1.0f;
        public float Range = 10.0f;
        public float Size = 1.0f;
        public bool BounceWalls = false;
        public int Damage = 10;
        public float DamageForce = 1.0f;
        public float EnergyCost = 50;
    }
}
