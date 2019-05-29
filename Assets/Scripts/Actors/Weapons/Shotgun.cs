using GFun;
using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon
{
    public WeaponIds WeaponId;
    public string DisplayName;
    public AudioClip FireSound;
    public AmmoType AmmoType => AmmoType.Shell;
    public int Level => 1;
    public string Name => DisplayName;
    public int AmmoCount => ammoProvider_.GetCurrentAmount(AmmoType);
    public WeaponIds Id => WeaponId;
    public Vector3 LatestFiringDirection { get; private set; }
    public float LatestFiringTimeUnscaled { get; private set; }
    public float AngleSpread = 20;
    public int BulletCount = 5;
    public float Cooldown = 0.3f;
    public PlainBulletSettings BulletSettings;

    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    IPhysicsActor forceReceiver_;
    IAmmoProvider ammoProvider_;
    CameraShake cameraShake_;
    Transform transform_;
    float cd_;
    bool awaitingRelease_;

    private void Awake()
    {
        bulletPool_ = SceneGlobals.Instance.ElongatedBulletPool;
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

        if (!ammoProvider_.TryUseAmmo(AmmoType, BulletCount))
            return;

        cd_ = Time.unscaledTime + Cooldown;
        awaitingRelease_ = true;

        LatestFiringDirection = firingDirection;
        LatestFiringTimeUnscaled = Time.unscaledTime;

        float angle = -AngleSpread * 0.5f;
        float angleStep = AngleSpread / (BulletCount - 1);
        float angleMaxVariation = 10;
        for (int j = 0; j < BulletCount; ++j)
        {
            float positionRandomOffset = Random.value * 0.35f;
            var position = transform_.position + firingDirection * (0.375f + positionRandomOffset);

            float angleOffset = angle + Random.value * angleMaxVariation;
            angle += angleStep;
            var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * firingDirection;
            Fire(position, offsetDirection);

            forceReceiver_.SetMinimumForce(-firingDirection * 3);
        }

        var particleCenter = transform_.position + firingDirection * 0.5f;
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 5);
        audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
        cameraShake_.SetMinimumShake(0.75f);
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
        var bulletSettings = BulletSettings;

        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (PlainBulletScript)bullet.GetComponent(typeof(PlainBulletScript));
        bulletScript.Init(position, direction, bulletSettings);
        bullet.SetActive(true);
    }
}
