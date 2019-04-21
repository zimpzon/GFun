using System.Collections;
using UnityEngine;

public class FireBatController : MonoBehaviour
{
    public AudioClip FireSound;

    IMovableActor movable_;
    ISensingActor senses_;
    IEnemy me_;
    IPhysicsActor physicsActor_;
    Vector3 dir_;
    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    float latestFiringTime_;
    float coolDownEnd_;
    float reloadEnd_;
    int pendingShots_;

    void Start()
    {
        movable_ = GetComponent<IMovableActor>();
        senses_ = GetComponent<ISensingActor>();
        senses_.LookForPlayerLoS(true, maxDistance: 6);
        me_ = GetComponent<IEnemy>();
        physicsActor_ = GetComponent<IPhysicsActor>();
        bulletPool_ = SceneGlobals.Instance.EnemyBullet1Pool;
        audioManager_ = SceneGlobals.Instance.AudioManager;

        StartCoroutine(AI());
    }

    void Fire(Vector3 position, Vector3 direction)
    {
        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (EnemyBullet1Script)bullet.GetComponent(typeof(EnemyBullet1Script));
        bulletScript.Init(me_, position, direction, range: 15, speed: 5, damage: 1);
        bullet.SetActive(true);

        audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
    }

    static WaitForSeconds ShootDelay = new WaitForSeconds(0.3f);

    void CheckFire(float time)
    {
        if (time < reloadEnd_ || me_.IsDead)
            return;
         
        bool hasRecentlySeenPlayer = senses_.GetPlayerLatestKnownPositionAge() < 1.0f;
        if (hasRecentlySeenPlayer && pendingShots_ == 0)
            pendingShots_ = 3;

        if (pendingShots_ > 0)
        {
            if (time > coolDownEnd_)
            {
                var myCenter = movable_.GetPosition() + Vector3.up * 0.5f;
                var playerCenter = senses_.GetPlayerLatestKnownPosition(PlayerPositionType.Center);
                var directionToPlayer = (playerCenter - myCenter).normalized;
                var bulletStartPos = myCenter + directionToPlayer * 0.2f;
                var bulletDirection = (playerCenter - bulletStartPos).normalized;

                float angleOffset = (Random.value - 0.5f) * 15;
                var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * bulletDirection;
                Fire(bulletStartPos, offsetDirection);

                coolDownEnd_ = time + 0.3f;
                if (--pendingShots_ == 0)
                    reloadEnd_ = time + 2.0f+ Random.value;
            }
        }
    }

    IEnumerator AI()
    {
        float baseSpeed = movable_.GetSpeed();

        while (true)
        {
            var pos = movable_.GetPosition();
            var direction = CollisionUtil.GetRandomFreeDirection(pos) * (Random.value * 0.8f + 0.1f);
            movable_.MoveTo(pos + direction);

            float endTime = Time.time + 4 + Random.value;
            while (true)
            {
                if (me_.IsDead)
                {
                    float explosionDelay = Time.time + 2.0f + Random.value * 0.5f;
                    while (Time.time < explosionDelay)
                    {
                        bool flashOn = ((int)(Time.time * 10) & 1) == 0;
                        me_.DoFlash(flashOn ? 0.0f : 10.0f, 1000);

                        yield return null;
                    }

                    me_.DoFlash(-0.25f, 0);

                    var deathPos = movable_.GetCenter();
                    MapScript.Instance.ExplodeWalls(deathPos, worldRadius: 2, particlesAtDestroyedWallsOnly: false);

                    ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, deathPos, 10);
                    ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, deathPos, 1);
                    ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, deathPos, 10);
                    yield break;
                }

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
