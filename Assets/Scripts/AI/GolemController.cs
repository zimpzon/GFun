using Apex.AI.Components;
using Apex.Examples.AI;
using Apex.Examples.AI.Game;
using MEC;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class GolemController : EntityComponentBase
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
            AudioManager.Instance.PlaySfxClip(AppearSound, 1);
    }

    void Activate(bool activate)
    {
        collider_.enabled = activate;
        aiPath_.enabled = activate;
        utilityAi_.enabled = activate;
        mySenses_.SetLookForPlayerLoS(true, 12);
    }

    private new void Start()
    {
        base.Start();
        Activate(false);
        transform.position = Vector3.right * 1000;
        GameEvents.OnPlayerKilled += GameEvents_OnPlayerKilled;
        Appear();
    }

    void Appear()
    {
        Timing.RunCoroutine(AppearCo().CancelWith(this.gameObject));
    }

    IEnumerator<float> AppearCo()
    {
        yield return Timing.WaitForSeconds(10 + Random.value * 10);

        PlayerInfoScript.Instance.ShowInfo("A Horrifying Sound Is Heard In The Distance", Color.red);

        AudioManager.Instance.PlaySfxClip(AppearSound, 1);
        yield return Timing.WaitForSeconds(6);

        var playerPos = AiBlackboard.Instance.PlayerPosition;
        Vector3 appearPos;
        if (playerPos.x > MapBuilder.WorldCenter.x)
            appearPos = MapUtil.GetRightmostFreeCell();
        else
            appearPos = MapUtil.GetLeftmostFreeCell();

        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.PlayerLand, 1, 0.1f);
        Activate(true);

        var startOffset = new Vector3(0, 1, -12);
        float t = 1;
        while (t >= 0)
        {
            CameraShake.Instance.SetMinimumShake(1.0f);

            var pos = appearPos + startOffset * t;
            transform_.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, t * 500));

            t -= Time.unscaledDeltaTime * 2;
            yield return 0;
        }

        transform_.SetPositionAndRotation(appearPos, Quaternion.identity);
        MapScript.Instance.TriggerExplosion(appearPos, 2);

        PlayerInfoScript.Instance.ShowInfo($"{me_.Name} Has Arrived!", Color.red);
        aiCoHandle_ = Timing.RunCoroutine(AICo().CancelWith(this.gameObject));
    }

    IEnumerator<float> AICo()
    {
        yield return Timing.WaitForSeconds(1 + Random.value * 1);

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
                    AudioManager.Instance.PlaySfxClip(AppearSound, maxInstances: 3, 0, 1.5f);
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

        deathDetected_ = true;
    }

    private void Update()
    {
        if (me_.IsDead && !deathDetected_)
            OnDeath();
    }
}
