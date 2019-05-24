using GFun;
using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon
{
    public WeaponIds WeaponId;
    public string DisplayName;
    public AudioClip FireSound;
    public AmmoType AmmoType => AmmoType.Bullets;
    public int Level => 1;
    public int AmmoCount => 100;
    public int AmmoMax => 100;
    public string Name => DisplayName;
    public WeaponIds Id => WeaponId;
    public Vector3 LatestFiringDirection { get; private set; }
    public float LatestFiringTimeUnscaled { get; private set; }
    public float AngleSpread = 30;
    public int BulletCount = 5;
    public float Cooldown = 0.3f;
    public PlainBulletSettings BulletSettings;

    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    IPhysicsActor forceReceiver_;
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

    public void SetOwner(IPhysicsActor forceReceiver)
    {
        forceReceiver_ = forceReceiver;
    }

    public void OnTriggerDown(Vector3 firingDirection)
    {
        if (Time.unscaledTime < cd_ || awaitingRelease_)
            return;

        cd_ = Time.unscaledTime + Cooldown;
        awaitingRelease_ = true;

        var position = transform_.position + firingDirection * 0.5f;
        LatestFiringDirection = firingDirection;
        LatestFiringTimeUnscaled = Time.unscaledTime;

        float angle = -AngleSpread * 0.5f;
        float angleStep = AngleSpread / (BulletCount - 1);
        float angleMaxVariation = 10;
        for (int j = 0; j < BulletCount; ++j)
        {
            float angleOffset = angle + Random.value * angleMaxVariation;
            angle += angleStep;
            var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * firingDirection;
            Fire(position, offsetDirection);

            audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
            cameraShake_.SetMinimumShake(0.75f);

            forceReceiver_.SetMinimumForce(-firingDirection * 3);

            var particleCenter = position + firingDirection * 0.3f;
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 5);
        }
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
