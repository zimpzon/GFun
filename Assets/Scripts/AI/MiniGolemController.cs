﻿using Apex.Examples.AI;
using Apex.Examples.AI.Game;
using MEC;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class MiniGolemController : EntityComponentBase
{
    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Collider2D collider_;
    Transform transform_;
    EnemyScript enemyScript_;
    CoroutineHandle aiCoHandle_;

    public override EntityType AiType => EntityType.FleeingBat;

    private new void Awake()
    {
        collider_ = GetComponent<Collider2D>();
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();
        transform_ = transform;
        enemyScript_ = GetComponent<EnemyScript>();

        base.Awake();
    }

    void Activate(bool activate)
    {
        collider_.enabled = activate;
        mySenses_.SetLookForPlayerLoS(activate, 12);
    }

    private new void Start()
    {
        base.Start();

        Activate(true);
        aiCoHandle_ = Timing.RunCoroutine(AICo().CancelWith(this.gameObject));
    }

    public void Run()
    {
        run_ = true;
    }

    bool run_;

    IEnumerator<float> AICo()
    {
        Activate(true);
        collider_.enabled = false;

        transform_.position = Vector3.right * 1000;
        while (!run_)
            yield return 0;

        yield return Timing.WaitForSeconds(2.0f + Random.value * 20);

        var basePos = AiBlackboard.Instance.PlayerPosition;
        var startOffset = new Vector3(0, 1, -12);

        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.PlayerLand, 3, 0.1f, 1.8f);

        float t = 1;
        while (t >= 0)
        {
            CameraShake.Instance.SetMinimumShake(0.3f);

            var pos = basePos + startOffset * t;
            transform_.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, t * 500));

            t -= Time.unscaledDeltaTime * 2f;
            yield return 0;
        }

        transform_.SetPositionAndRotation(basePos, Quaternion.identity);
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.WallDestructionParticles, basePos, 10);
        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.LargeExplosion1, 3, 0.1f, 1.2f);
        SceneGlobals.Instance.CameraShake.SetMinimumShake(1.0f);

        collider_.enabled = true;

        while (true)
        {
            bool recentlySeenPlayer = mySenses_.GetPlayerLatestKnownPositionAge() < 2.0f;
            myMovement_.MoveTo(AiBlackboard.Instance.PlayerPosition);
            yield return Timing.WaitForSeconds(0.05f);
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
}
