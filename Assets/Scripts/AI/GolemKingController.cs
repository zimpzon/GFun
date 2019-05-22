using Apex.AI.Components;
using Apex.Examples.AI;
using Apex.Examples.AI.Game;
using MEC;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class GolemKingController : EntityComponentBase
{
    public AudioClip AppearSound;
    public DamagingParticleSystem DefaultProjectiles;
    public ParticleSystem RageTelegraphParticles;

    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Collider2D collider_;
    AIPath aiPath_;
    UtilityAIComponent utilityAi_;
    Transform transform_;
    EnemyScript enemyScript_;
    CoroutineHandle aiCoHandle_;

    public override EntityType AiType => EntityType.FleeingBat;

    private new void Awake()
    {
        collider_ = GetComponent<Collider2D>();
        aiPath_ = GetComponent<AIPath>();
        utilityAi_ = GetComponent<UtilityAIComponent>();
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();
        transform_ = transform;
        enemyScript_ = GetComponent<EnemyScript>();

        base.Awake();
    }

    private void GameEvents_OnPlayerKilled(IEnemy enemy)
    {
        if (enemy == me_)
            AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 0.8f);
    }

    void Activate(bool activate)
    {
        collider_.enabled = activate;
        aiPath_.enabled = activate;
        utilityAi_.enabled = activate;
        mySenses_.SetLookForPlayerLoS(activate, 12);
    }

    private new void Start()
    {
        base.Start();
        GameEvents.OnPlayerKilled += GameEvents_OnPlayerKilled;

        Activate(true);
        aiCoHandle_ = Timing.RunCoroutine(AICo().CancelWith(this.gameObject));
    }

    IEnumerator<float> AICo()
    {
        yield return Timing.WaitForSeconds(2);

        AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 0.75f);
        yield return Timing.WaitForSeconds(0.1f);
        AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 0.8f);

        float rageTimer = 0.0f;

        while (true)
        {
            bool recentlySeenPlayer = mySenses_.GetPlayerLatestKnownPositionAge() < 2.0f;
            DefaultProjectiles.EnableEmission = recentlySeenPlayer;

            if (recentlySeenPlayer)
            {
                rageTimer += Time.deltaTime;
                if (rageTimer > (me_.LifePct * 4) && me_.LifePct < 0.8f)
                {
                    // Telegraph to player
                    enemyScript_.gameObject.layer = SceneGlobals.Instance.EnemyLayer;
                    enemyScript_.EnableAiPath(false);
                    AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 0.8f);
                    myMovement_.StopMove();
                    DefaultProjectiles.EnableEmission = false;
                    var rageEmission = RageTelegraphParticles.emission;
                    rageEmission.enabled = true;

                    yield return Timing.WaitForSeconds(1);
                    rageEmission.enabled = false;

                    // Rage
                    Vector2 playerDir = (AiBlackboard.Instance.PlayerPosition - transform_.position).normalized;
                    myPhysics_.AddForce(playerDir * 200);
                    rageTimer = 0;

                    yield return Timing.WaitForSeconds(2);

                    // Stop rage
                    MapScript.Instance.TriggerExplosion(transform_.position, 3, false, me_, damageSelf: false);
                    enemyScript_.gameObject.layer = SceneGlobals.Instance.EnemyNoWallsLayer;
                    enemyScript_.EnableAiPath(true);

                    yield return Timing.WaitForSeconds(0.3f);
                }
            }

            myMovement_.MoveTo(AiBlackboard.Instance.PlayerPosition);
            yield return 0;
        }
    }

    bool deathDetected_;
    void OnDeath()
    {
        Timing.KillCoroutines(aiCoHandle_);
        DefaultProjectiles.EnableEmission = false;
        var rageEmission = RageTelegraphParticles.emission;
        rageEmission.enabled = false;
        enemyScript_.gameObject.layer = SceneGlobals.Instance.DeadEnemyLayer;

        deathDetected_ = true;
    }

    private void Update()
    {
        if (me_.IsDead && !deathDetected_)
            OnDeath();
    }
}
