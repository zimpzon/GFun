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

    IEnumerator FireCo()
    {
        isFiring_ = true;

        for (int i = 0; i < GunSettings.BurstCount; ++i)
        {
            var direction = transform.right;
            var position = transform.position + direction * 0.5f;
            Fire(position, direction);

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

        audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
        cameraShake_.SetMinimumShake(0.5f);
        forceCallback_?.Invoke(-direction * 4);

        var particleCenter = position + direction * 0.3f;
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, particleCenter, 1);
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, particleCenter, 5);
    }
}
