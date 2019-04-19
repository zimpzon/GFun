using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    public static ParticleScript Instance;

    public bool ParticlesUseUnscaledTime = false;
    public ParticleSystem WallDestructionParticles;
    public ParticleSystem BulletFizzleParticles;
    public ParticleSystem MuzzleFlashParticles;
    public ParticleSystem MuzzleSmokeParticles;
    public ParticleSystem CharacterSelectedParticles;
    public ParticleSystem PlayerLandParticles;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        // ?
        SetUnscaledTime(WallDestructionParticles, ParticlesUseUnscaledTime);
        SetUnscaledTime(BulletFizzleParticles, ParticlesUseUnscaledTime);
    }

    public static void EmitAtPosition(ParticleSystem system, Vector3 position, int count)
    {
        system.transform.position = position;
        system.Emit(count);
    }

    void SetUnscaledTime(ParticleSystem particleSystem, bool value)
    {
        var main = particleSystem.main;
        main.useUnscaledTime = value;
    }
}
