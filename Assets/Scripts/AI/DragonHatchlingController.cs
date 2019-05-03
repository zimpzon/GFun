using Apex.Examples.AI;
using Apex.Examples.AI.Game;
using UnityEngine;

public class DragonHatchlingController : EntityComponentBase
{
    public AudioClip FireSound;
    public float ShootRange = 10;

    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Transform transform_;
    GameObjectPool bulletPool_;
    AudioManager audioManager_;
    float latestFiringTime_;
    float coolDownEnd_;
    float reloadEnd_;
    int pendingShots_;

    public override EntityType AiType => EntityType.FleeingBat;

    private new void Awake()
    {
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 12);
        mySenses_.SetLookForNearbyCover(true, maxDistance: 4);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();
        bulletPool_ = SceneGlobals.Instance.DragonHatchlingProjectilePool;
        audioManager_ = SceneGlobals.Instance.AudioManager;
        transform_ = transform;

        base.Awake();
    }

    private void Update()
    {
        CheckFire(Time.time);
    }

    void Fire(Vector3 position, Vector3 direction)
    {
        var bullet = bulletPool_.GetFromPool();
        var bulletScript = (EnemyBullet1Script)bullet.GetComponent(typeof(EnemyBullet1Script));
        bulletScript.Init(me_, position, direction, range: 15, speed: 5, damage: 1);
        bullet.SetActive(true);
        float rotationDegrees = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, rotationDegrees);

        audioManager_.PlaySfxClip(FireSound, 1, 0.1f);
    }

    void CheckFire(float time)
    {
        if (time < reloadEnd_ || me_.IsDead)
            return;

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
                    reloadEnd_ = time + 2.0f + Random.value;
            }
        }
    }
}