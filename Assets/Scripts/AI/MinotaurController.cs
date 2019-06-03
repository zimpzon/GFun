using MEC;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class MinotaurController : MonoBehaviour
{
    public float ShootRange = 10;
    public AudioClip FireSound;

    GameObjectPool bulletPool_;
    AudioManager audioManager_;

    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Collider2D collider_;
    AIPath aiPath_;
    Transform transform_;
    EnemyScript enemyScript_;
    CoroutineHandle aiCoHandle_;
    float latestFiringTime_;
    float coolDownEnd_;
    float reloadEnd_;
    int pendingShots_;

    private void Awake()
    {
        collider_ = GetComponent<Collider2D>();
        aiPath_ = GetComponent<AIPath>();
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();
        transform_ = transform;
        enemyScript_ = GetComponent<EnemyScript>();
        bulletPool_ = SceneGlobals.Instance.MinotaurProjectilePool;
        audioManager_ = SceneGlobals.Instance.AudioManager;
    }

    void Activate(bool activate)
    {
        collider_.enabled = activate;
        aiPath_.enabled = activate;
        mySenses_.SetLookForPlayerLoS(activate, 12);
    }

    private void Start()
    {
        Activate(true);
        aiCoHandle_ = Timing.RunCoroutine(AICo().CancelWith(this.gameObject));
    }

    IEnumerator<float> AICo()
    {
        yield return Timing.WaitForSeconds(1 + Random.value * 1);

        float rageTimer = 0.0f;

        while (true)
        {
            bool recentlySeenPlayer = mySenses_.GetPlayerLatestKnownPositionAge() < 2.0f;

            CheckFire(Time.time);
            myMovement_.MoveTo(AiBlackboard.Instance.PlayerPosition);
            yield return 0;
        }
    }

    bool deathDetected_;
    void OnDeath()
    {
        Timing.KillCoroutines(aiCoHandle_);
        enemyScript_.gameObject.layer = SceneGlobals.Instance.DeadEnemyLayer;

        deathDetected_ = true;
    }

    private void Update()
    {
        if (me_.IsDead && !deathDetected_)
            OnDeath();
    }

    void CheckFire(float time)
    {
        if (time < reloadEnd_ || me_.IsDead)
        {
            return;
        }
        else if (time < reloadEnd_ + 0.5f)
        {
            enemyScript_.IsAttacking = true;
            return;
        }

        var myPos = transform_.position;

        float sqrDistanceToPlayer = (AiBlackboard.Instance.PlayerPosition - myPos).sqrMagnitude;
        bool withinRange = sqrDistanceToPlayer < ShootRange * ShootRange;

        if (withinRange && pendingShots_ == 0)
            pendingShots_ = 1;

        if (pendingShots_ > 0)
        {
            if (time > coolDownEnd_)
            {
                var myCenter = myPos + Vector3.up * 0.5f;
                var playerCenter = AiBlackboard.Instance.PlayerPosition + Vector3.up * 0.5f;
                var directionToPlayer = (playerCenter - myCenter).normalized;
                var bulletStartPos = myCenter + directionToPlayer * 0.2f;
                var bulletDirection = (playerCenter - bulletStartPos).normalized;

                float angleOffset = (Random.value - 0.5f) * 10;
                var offsetDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * bulletDirection;
                Fire(bulletStartPos, offsetDirection);

                coolDownEnd_ = time + 0.3f;
                if (--pendingShots_ == 0)
                    reloadEnd_ = time + 2.0f + Random.value * 0.5f;
            }
        }
    }

    void Fire(Vector3 position, Vector3 direction)
    {
        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (MinotaurProjectileScript)bullet.GetComponent(typeof(MinotaurProjectileScript));
        bulletScript.Init(me_, position, direction, range: 20, speed: 5 + Random.value, turnSpeed: 1.0f + Random.value * 0.25f, damage: 2, collideWalls: true);
        bullet.SetActive(true);
        float rotationDegrees = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, rotationDegrees - 90);

        audioManager_.PlaySfxClip(FireSound, 3, 0.1f);
        enemyScript_.IsAttacking = false;
    }
}
