using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    public bool ParticlesUseUnscaledTime = false;
    public ParticleSystem WallDestructionParticles;
    public ParticleSystem BulletFizzleParticles;

    public void Start()
    {
        SetUnscaledTime(WallDestructionParticles, ParticlesUseUnscaledTime);
        SetUnscaledTime(BulletFizzleParticles, ParticlesUseUnscaledTime);
    }

    void SetUnscaledTime(ParticleSystem particleSystem, bool value)
    {
        var main = particleSystem.main;
        main.useUnscaledTime = value;
    }
}
