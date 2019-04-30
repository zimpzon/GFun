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
    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Collider2D collider_;
    AIPath aiPath_;
    UtilityAIComponent utilityAi_;

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

        base.Awake();
    }

    void Activate(bool activate)
    {
        collider_.enabled = activate;
        aiPath_.enabled = activate;
        utilityAi_.enabled = activate;
    }

    private new void Start()
    {
        base.Start();
        Activate(false);
        Appear();
    }

    void Appear()
    {
        Timing.RunCoroutine(AppearCo());
    }

    IEnumerator<float> AppearCo()
    {
        yield return Timing.WaitForSeconds(10 + Random.value * 10);

        AudioManager.Instance.PlaySfxClip(AppearSound, 1);
        yield return Timing.WaitForSeconds(3);

        var playerPos = AiBlackboard.Instance.PlayerPosition;

        Vector3 appearPos;
        if (playerPos.x > MapBuilder.WorldCenter.x)
            appearPos = MapUtil.GetLeftmostFreeCell();
        else
            appearPos = MapUtil.GetRightmostFreeCell();

        MapScript.Instance.TriggerExplosion(appearPos, 2);
        transform.position = appearPos;

        Activate(true);
    }

    private void Update()
    {
        myMovement_.MoveTo(AiBlackboard.Instance.PlayerPosition);
    }
}
