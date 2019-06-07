using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElfController : MonoBehaviour
{
    public AudioClip FireSound;
    public GameObject Weapon;

    IMovableActor movable_;
    ISensingActor senses_;
    IEnemy me_;
    IPhysicsActor physicsActor_;
    Vector3 dir_;
    AudioManager audioManager_;
    float latestFiringTime_;
    float coolDownEnd_;
    float reloadEnd_;
    int pendingShots_;

    List<GameObjectPool> bulletPool_;
    List<Action<Vector3>> triggerActions;
    private void Start()
    {
        movable_ = GetComponent<IMovableActor>();
        senses_ = GetComponent<ISensingActor>();
        senses_.SetLookForPlayerLoS(true, maxDistance: 20);
        me_ = GetComponent<IEnemy>();
        physicsActor_ = GetComponent<IPhysicsActor>();
        bulletPool_ = new List<GameObjectPool>() { SceneGlobals.Instance.ElfIceArrowProjectilePool, SceneGlobals.Instance.ElfFireArrowProjectilePool };
        triggerActions = new List<Action<Vector3>>() { null, FireArrowAction };
        audioManager_ = SceneGlobals.Instance.AudioManager;

        StartCoroutine(AI());
    }

    private void FireArrowAction(Vector3 pos)
    {
            MapScript.Instance.TriggerExplosion(pos, worldRadius: 1.9f, damageWallsOnly: false);

            ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, pos, 10);
            ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, pos, 1);
            ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, pos, 10);
    }

    void Fire(Vector3 position, Vector3 direction)
    {
        int randBullet = UnityEngine.Random.Range(0, bulletPool_.Count);
        var bullet = bulletPool_[randBullet].GetFromPool();
        var bulletScript = (EnemyBullet1Script)bullet.GetComponent(typeof(EnemyBullet1Script));
        bulletScript.Init(me_, position, direction, range: 25, speed: 7, damage: 2, collideWalls: true, triggerActions[randBullet]);
        bullet.SetActive(true);
        float rotationDegrees = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, rotationDegrees - 90);
        Weapon.transform.rotation = Quaternion.Euler(0, 0, rotationDegrees - 45);
        audioManager_.PlaySfxClip(FireSound, 3, 0.1f);
    }

    static WaitForSeconds ShootDelay = new WaitForSeconds(0.3f);

    void CheckFire(float time)
    {
        if (time < reloadEnd_ || me_.IsDead)
            return;

        bool hasRecentlySeenPlayer = senses_.GetPlayerLatestKnownPositionAge() < 2.0f;
        if (hasRecentlySeenPlayer && pendingShots_ == 0)
            pendingShots_ = 2;

        if (pendingShots_ > 0)
        {
            if (time > coolDownEnd_)
            {
                var myCenter = movable_.GetPosition() + Vector3.up * 0.5f;
                var playerCenter = senses_.GetPlayerLatestKnownPosition(PlayerPositionType.Center);
                var directionToPlayer = (playerCenter - myCenter).normalized;
                var bulletStartPos = myCenter + directionToPlayer * 0.2f;
                var bulletDirection = (playerCenter - bulletStartPos).normalized;

                float angleOffset = (UnityEngine.Random.value - 0.5f) * 15;
                var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * bulletDirection;
                Fire(bulletStartPos, offsetDirection);

                coolDownEnd_ = time + 0.4f;
                if (--pendingShots_ == 0)
                    reloadEnd_ = time + 3.0f + UnityEngine.Random.value;
            }
        }
    }

    IEnumerator AI()
    {
        float baseSpeed = movable_.GetSpeed();

        while (true)
        {
            var pos = movable_.GetPosition();
            var direction = CollisionUtil.GetRandomFreeDirection(pos) * (UnityEngine.Random.value * 0.8f + 0.1f);
            movable_.MoveTo(pos + direction);

            float endTime = Time.time + 3 + UnityEngine.Random.value * 2;
            while (true)
            {
                if (me_.IsDead)
                    yield break;

                float time = Time.time;
                CheckFire(time);

                if (movable_.MoveTargetReached() || time > endTime)
                    break;

                yield return null;
            }

            yield return null;
        }
    }

}