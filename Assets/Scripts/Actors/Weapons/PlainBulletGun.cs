using GFun;
using System;
using System.Collections;
using UnityEngine;

public class PlainBulletGun : MonoBehaviour, IWeapon
{
    public AudioClip FireSound;
    public AmmoType AmmoType => AmmoType.Bullets;
    public int Level => GunSettings.Level;
    public int AmmoCount => GunSettings.AmmoCount;
    public int AmmoMax => GunSettings.AmmoMax;

    public PlainBulletGunSettings GunSettings;
    public PlainBulletSettings BulletSettings;

    public static bool EffectsOn = true;

    static readonly float[] FiringAngleOffsetsSingle = new float[] { 0 };
    static readonly float[] FiringAngleOffsetsDual = new float[] { -5, 5 };
    static readonly float[] FiringAngleOffsetsTripple = new float[] { -5, 0, 5 };
    static readonly float[] FiringAngleOffsetsQuad = new float[] { -10, -5, 5, 10 };

    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    Action<Vector3> forceCallback_;
    CameraShake cameraShake_;
    bool triggerIsDown_;
    bool awaitingRelease_;
    bool isFiring_;
    WaitForSecondsRealtime shotDelay_;

    private void Start()
    {
        bulletPool_ = SceneGlobals.Instance.ElongatedBulletPool;
        audioManager_ = SceneGlobals.Instance.AudioManager;
        cameraShake_ = SceneGlobals.Instance.CameraShake;

        shotDelay_ = new WaitForSecondsRealtime(GunSettings.TimeBetweenShots);
    }

    public void SetForceCallback(Action<Vector3> forceCallback)
        => forceCallback_ = forceCallback;

    public void OnTriggerDown()
    {
        if (awaitingRelease_ || GunSettings.FiringMode == FiringMode.None)
            return;

        triggerIsDown_ = true;
        awaitingRelease_ = GunSettings.FiringMode == FiringMode.Single;

        if (!isFiring_)
            StartCoroutine(FireCo());
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

    IEnumerator FireCo()
    {
        isFiring_ = true;

        for (int i = 0; i < GunSettings.BurstCount; ++i)
        {
            var direction = transform.right;
            var position = transform.position + direction * 0.5f;

            var angleOffsets = GetFiringAngleOffsets(GunSettings.FiringSpread, 1.0f);
            for (int j = 0; j < angleOffsets.Length; ++j)
            {
                var offsetDirection = Quaternion.AngleAxis(angleOffsets[j], Vector3.forward) * direction;
                Fire(position, offsetDirection);
            }

            yield return shotDelay_;
        }

        if (GunSettings.FiringMode == FiringMode.Auto && triggerIsDown_)
            StartCoroutine(FireCo());
        else
            isFiring_ = false;
    }

    void Fire(Vector3 position, Vector3 direction)
    {
        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (PlainBulletScript)bullet.GetComponent(typeof(PlainBulletScript));
        bulletScript.Init(position, direction, BulletSettings);
        bullet.SetActive(true);

        if (EffectsOn)
        {
            audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
            cameraShake_.SetMinimumShake(0.75f);
            forceCallback_?.Invoke(-direction * 2);

            var particleCenter = position + direction * 0.3f;
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 5);
        }
    }
}
