using GFun;
using UnityEngine;

public class RocketLauncher : MonoBehaviour, IWeapon
{
    public WeaponIds WeaponId;
    public string DisplayName;
    public AudioClip FireSound;
    public float FireSoundPitch = 1.0f;
    public float FireSoundPitchVariation = 0.1f;
    public AmmoType AmmoType => AmmoType.Explosive;
    public float Cooldown = 1.0f;
    public PlainBulletSettings RocketSettings;
    public int Level => 5;
    public string Name => DisplayName;
    public int AmmoCount => ammoProvider_.GetCurrentAmount(AmmoType);
    public WeaponIds Id => WeaponId;
    public Vector3 LatestFiringDirection { get; private set; }
    public float LatestFiringTimeUnscaled { get; private set; }

    GameObjectPool rocketPool_;
    AudioManager audioManager_;
    IPhysicsActor forceReceiver_;
    IAmmoProvider ammoProvider_;
    CameraShake cameraShake_;
    Transform transform_;
    float cd_;
    bool awaitingRelease_;

    private void Awake()
    {
        rocketPool_ = SceneGlobals.Instance.RocketPool;
        audioManager_ = SceneGlobals.Instance.AudioManager;
        cameraShake_ = SceneGlobals.Instance.CameraShake;
        transform_ = transform;
    }

    public void SetAmmoProvider(IAmmoProvider ammoProvider) => ammoProvider_ = ammoProvider;
    public void SetOwner(IPhysicsActor forceReceiver) => forceReceiver_ = forceReceiver;

    public void OnTriggerDown(Vector3 firingDirection)
    {
        if (Time.unscaledTime < cd_ || awaitingRelease_)
            return;

        if (!ammoProvider_.TryUseAmmo(AmmoType, 1))
            return;

        cd_ = Time.unscaledTime + Cooldown;
        awaitingRelease_ = true;

        LatestFiringDirection = firingDirection;
        LatestFiringTimeUnscaled = Time.unscaledTime;

        var position = transform_.position + firingDirection * 0.5f;
        Fire(position, firingDirection);
        forceReceiver_.SetMinimumForce(-firingDirection * 3);

        var particleCenter = transform_.position + firingDirection * 0.5f;
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 10);
        audioManager_.PlaySfxClip(FireSound, 2, FireSoundPitchVariation, FireSoundPitch);
        cameraShake_.SetMinimumShake(1.0f);
    }

    public void OnTriggerUp()
    {
        awaitingRelease_ = false;
    }

    public Vector3 GetMuzzlePosition(Vector3 target)
    {
        var direction = (target - transform_.position).normalized;
        return transform_.position + direction * 0.5f;
    }

    void Fire(Vector3 position, Vector3 direction, bool powerShot = false)
    {
        var rocket = rocketPool_.GetFromPool();
        var rocketScript = (RocketScript)rocket.GetComponent(typeof(RocketScript));
        rocketScript.Init(position, direction, RocketSettings);
        rocket.SetActive(true);
    }
}
