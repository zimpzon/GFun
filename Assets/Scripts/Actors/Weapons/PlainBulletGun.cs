using GFun;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class PlainBulletGun : MonoBehaviour, IWeapon
{
    public WeaponIds WeaponId;
    public string DisplayName;
    public AudioClip FireSound;
    public AmmoType AmmoType => AmmoType.Bullets;
    public int Level => GunSettings.Level;
    public int AmmoCount => GunSettings.AmmoCount;
    public int AmmoMax => GunSettings.AmmoMax;
    public string Name => DisplayName;
    public WeaponIds Id => WeaponId;
    public Vector3 LatestFiringDirection => latestFiringDirection_;
    public float LatestFiringTimeUnscaled => latestFiringTime_;

    public PlainBulletGunSettings GunSettings;
    public PlainBulletSettings BulletSettings;
    public PlainBulletSettings PowerBulletSettings;

    public static bool EffectsOn = true;

    static readonly float[] FiringAngleOffsetsSingle = new float[] { 0 };
    static readonly float[] FiringAngleOffsetsDual = new float[] { -5, 5 };
    static readonly float[] FiringAngleOffsetsTripple = new float[] { -5, 0, 5 };
    static readonly float[] FiringAngleOffsetsQuad = new float[] { -10, -5, 5, 10 };

    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    IPhysicsActor forceReceiver_;
    CameraShake cameraShake_;
    bool triggerIsDown_;
    bool awaitingRelease_;
    bool isFiring_;
    Vector3 latestFiringDirection_;
    float latestFiringTime_ = float.MinValue;

    private void Awake()
    {
        bulletPool_ = SceneGlobals.Instance.ElongatedBulletPool;
        audioManager_ = SceneGlobals.Instance.AudioManager;
        cameraShake_ = SceneGlobals.Instance.CameraShake;
    }

    public void SetForceReceiver(IPhysicsActor forceReceiver)
        => forceReceiver_ = forceReceiver;

    public void OnTriggerDown(Vector3 firingDirection)
    {
        if (awaitingRelease_ || GunSettings.FiringMode == FiringMode.None)
            return;

        triggerIsDown_ = true;
        awaitingRelease_ = GunSettings.FiringMode == FiringMode.Single;
        latestFiringDirection_ = firingDirection;

        CheckCombo(firingDirection);
        if (!isFiring_)
            Timing.RunCoroutine(FireCo());
    }

    List<float> comboDegrees = new List<float> { 0, 180 };
    int idx;

    Vector3 prevComboDir;
    void CheckCombo(Vector3 direction)
    {
        float angle = Vector3.Angle(prevComboDir, direction);
        prevComboDir = direction;
        if (angle == comboDegrees[idx])
        {
            idx++;
            if (idx == comboDegrees.Count)
            {
                idx = 0;
                var position = transform.position + direction * 0.5f;
                Fire(position, direction, powerShot: true);
            }
        }
        else
        {
            idx = 0;
        }
    }

    public void OnTriggerUp()
    {
        triggerIsDown_ = false;
        awaitingRelease_ = false;
    }

    float[] GetFiringAngleOffsets(FiringSpread spread, float precision)
    {
        switch(spread)
        {
            case FiringSpread.Single: return FiringAngleOffsetsSingle;
            case FiringSpread.Dual: return FiringAngleOffsetsDual;
            case FiringSpread.Tripple: return FiringAngleOffsetsTripple;
            default: return FiringAngleOffsetsSingle;
        }
    }

    IEnumerator<float> FireCo()
    {
        isFiring_ = true;

        while (true)
        {
            for (int i = 0; i < GunSettings.BurstCount; ++i)
            {
                var direction = latestFiringDirection_;
                var position = transform.position + direction * 0.5f;

                var angleOffsets = GetFiringAngleOffsets(GunSettings.FiringSpread, 1.0f);
                for (int j = 0; j < angleOffsets.Length; ++j)
                {
                    float angleOffset = angleOffsets[j];
                    const float MaxDegreesOffsetAtLowestPrecision = 30.0f;
                    angleOffset += (Random.value - 0.5f) * (1.0f - GunSettings.Precision) * MaxDegreesOffsetAtLowestPrecision;
                    var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * direction;
                    Fire(position, offsetDirection);
                }

                // This must be realtime seconds and I'm not sure free version of MEC support realtime. So wait manually.
                float waitEndTime = Time.realtimeSinceStartup + GunSettings.TimeBetweenShots;
                while (Time.realtimeSinceStartup < waitEndTime)
                    yield return 0;
            }

            bool continueFiring = GunSettings.FiringMode == FiringMode.Auto && triggerIsDown_;
            if (!continueFiring)
                break;
        }

        isFiring_ = false;
    }

    void Fire(Vector3 position, Vector3 direction, bool powerShot = false)
    {
        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (PlainBulletScript)bullet.GetComponent(typeof(PlainBulletScript));
        bulletScript.Init(position, direction, powerShot ? PowerBulletSettings : BulletSettings);
        bullet.SetActive(true);

        latestFiringTime_ = Time.unscaledTime;

        if (EffectsOn)
        {
            audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
            cameraShake_.SetMinimumShake(0.75f);
            forceReceiver_.SetMinimumForce(-direction * 2);

            var particleCenter = position + direction * 0.3f;
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 5);
        }
    }
}
