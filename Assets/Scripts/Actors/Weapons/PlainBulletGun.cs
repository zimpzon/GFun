﻿using GFun;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class PlainBulletGun : MonoBehaviour, IWeapon
{
    public WeaponIds WeaponId;
    public string DisplayName;
    public AudioClip FireSound;
    public float FireSoundPitch = 1.0f;
    public float FireSoundPitchVariation = 0.1f;
    public AmmoType AmmoType => AmmoType.Bullet;
    public int Level => GunSettings.Level;
    public int AmmoCount => ammoProvider_.GetCurrentAmount(AmmoType);
    public string Name => DisplayName;
    public WeaponIds Id => WeaponId;
    public float Recoil = 1.0f;
    public Vector3 LatestFiringDirection => latestFiringDirection_;
    public float LatestFiringTimeUnscaled => latestFiringTime_;

    public PlainBulletGunSettings GunSettings;
    public PlainBulletSettings BulletSettings;

    static readonly float[] FiringAngleOffsetsSingle = new float[] { 0 };
    static readonly float[] FiringAngleOffsetsDual = new float[] { -5, 5 };
    static readonly float[] FiringAngleOffsetsTripple = new float[] { -5, 0, 5 };
    static readonly float[] FiringAngleOffsetsQuad = new float[] { -10, -5, 5, 10 };

    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    IPhysicsActor forceReceiver_;
    IAmmoProvider ammoProvider_;
    CameraShake cameraShake_;
    bool triggerIsDown_;
    bool awaitingRelease_;
    bool isFiring_;
    float penalty_;
    float timeEndFire_;
    Transform transform_;
    Vector3 latestFiringDirection_;
    float latestFiringTime_ = float.MinValue;

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
        if (awaitingRelease_ || GunSettings.FiringMode == FiringMode.None || AmmoCount == 0)
            return;

        triggerIsDown_ = true;
        awaitingRelease_ = GunSettings.FiringMode == FiringMode.Single;
        latestFiringDirection_ = firingDirection;
        if (!isFiring_)
        {
            float pauseTime = Time.unscaledTime - timeEndFire_;
            penalty_ = Mathf.Max(0, penalty_ - pauseTime * 3);
            Timing.RunCoroutine(FireCo().CancelWith(this.gameObject));
        }
    }

    public void OnTriggerUp()
    {
        triggerIsDown_ = false;
        awaitingRelease_ = false;
        timeEndFire_ = Time.unscaledTime;
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

    public Vector3 GetMuzzlePosition(Vector3 target)
    {
        var direction = (target - transform_.position).normalized;
        return transform_.position + direction * 0.5f;
    }

    IEnumerator<float> FireCo()
    {
        isFiring_ = true;

        while (true)
        {
            for (int i = 0; i < GunSettings.BurstCount; ++i)
            {
                var direction = latestFiringDirection_;
                var position = transform_.position + direction * 0.5f;

                var angleOffsets = GetFiringAngleOffsets(GunSettings.FiringSpread, 1.0f);
                int bulletCount = angleOffsets.Length;
                if (!ammoProvider_.TryUseAmmo(AmmoType, bulletCount))
                    yield break;

                for (int j = 0; j < bulletCount; ++j)
                {
                    float precisionPenalty = Mathf.Min(0.75f, Mathf.Max(0, -0.5f + penalty_ * 0.8f));
                    float precision = GunSettings.Precision - precisionPenalty;

                    float angleOffset = angleOffsets[j];
                    const float MaxDegreesOffsetAtLowestPrecision = 30.0f;
                    angleOffset += (Random.value - 0.5f) * (1.0f - precision) * MaxDegreesOffsetAtLowestPrecision;
                    var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * direction;
                    Fire(position, offsetDirection);
                }

                latestFiringTime_ = Time.unscaledTime;

                audioManager_.PlaySfxClip(FireSound, 1, FireSoundPitchVariation, FireSoundPitch);
                cameraShake_.SetMinimumShake(0.75f);

                forceReceiver_.SetMinimumForce(-direction * Recoil);

                var particleCenter = position + direction * 0.3f;
                ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
                ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 5);

                float timePenalty = Mathf.Min(0.2f, Mathf.Max(0, -0.5f + penalty_ * 0.4f));
                float waitEndTime = Time.realtimeSinceStartup + GunSettings.TimeBetweenShots + timePenalty;
                while (Time.realtimeSinceStartup < waitEndTime)
                    yield return 0;

                penalty_ = Mathf.Min(2.0f, penalty_ + 0.1f);
            }

            bool continueFiring = GunSettings.FiringMode == FiringMode.Auto && triggerIsDown_;
            if (!continueFiring)
                break;
        }

        isFiring_ = false;
    }

    void Fire(Vector3 position, Vector3 direction)
    {
        var bulletSettings = BulletSettings;

        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (PlainBulletScript)bullet.GetComponent(typeof(PlainBulletScript));
        bulletScript.Init(position, direction, bulletSettings);
        bullet.SetActive(true);
    }
}
