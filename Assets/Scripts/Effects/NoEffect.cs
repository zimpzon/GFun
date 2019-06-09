using System;

namespace Assets.Scripts.Effects
{
    public class NoEffect : IEffect
    {
        public eEffects Effect => eEffects.None;

        public float Value => 0.0f;

        public float Time { get => 0.0f; set => throw new NotImplementedException(); }
    }
}
