using Apex.Examples.AI;
using Apex.Examples.AI.Game;
using MEC;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class GolemKingController : EntityComponentBase
{
    public AudioClip AppearSound;
    public ParticleSystem RageTelegraphParticles;

    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Collider2D collider_;
    AIPath aiPath_;
    Transform transform_;
    EnemyScript enemyScript_;
    CoroutineHandle aiCoHandle_;

    public override EntityType AiType => EntityType.FleeingBat;

    private new void Awake()
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

        // 1) Follow player until damaged a bit
        while (me_.LifePct > 0.95)
        {
            myMovement_.MoveTo(AiBlackboard.Instance.PlayerPosition);
            yield return 0;
        }

        // 2) Call for help
        myMovement_.StopMove();
        AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 2.5f);
        yield return Timing.WaitForSeconds(0.1f);
        AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 2.0f);
        yield return Timing.WaitForSeconds(1.5f);

        GameEvents.RaiseGolemKingCallForHelp();

        // 3) Standard loop
        float rageTimer = 0.0f;

        while (true)
        {
            DebugLinesScript.Show("life", me_.Life);

            bool recentlySeenPlayer = mySenses_.GetPlayerLatestKnownPositionAge() < 2.0f;

            if (recentlySeenPlayer)
            {
                rageTimer += Time.deltaTime;
                if (rageTimer > (me_.LifePct * 4) && me_.LifePct < 0.8f)
                {
                    // Telegraph to player
                    enemyScript_.gameObject.layer = SceneGlobals.Instance.EnemyLayer;
                    enemyScript_.EnableAiPath(false);
                    AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 1.4f);
                    myMovement_.StopMove();
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
