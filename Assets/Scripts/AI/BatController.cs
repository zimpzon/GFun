﻿using System.Collections;
using UnityEngine;

public class BatController : MonoBehaviour
{
    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    Vector3 dir_;

    void Start()
    {
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();

        StartCoroutine(AI());
    }

    IEnumerator AI()
    {
        float baseSpeed = myMovement_.GetSpeed();

        while (true)
        {
            while (true)
            {
                if (me_.IsDead)
                    yield break;

                bool hasRecentlySeenPlayer = mySenses_.GetPlayerLatestKnownPositionAge() < 2.0f;
                if (!hasRecentlySeenPlayer)
                    break;

                myMovement_.SetSpeed(baseSpeed * 2);

                var target = mySenses_.GetPlayerLatestKnownPosition(PlayerPositionType.Tile);
                myMovement_.MoveTo(target);

                yield return null;
            }

            myMovement_.SetSpeed(baseSpeed);

            var pos = myMovement_.GetPosition();
            var direction = CollisionUtil.GetRandomFreeDirection(pos) * (Random.value * 0.8f + 0.1f);
            myMovement_.MoveTo(pos + direction);

            float endTime = Time.time + 4 + Random.value;
            while (true)
            {
                if (myMovement_.MoveTargetReached())
                    break;

                if (Time.time > endTime)
                    break;

                bool hasRecentlySeenPlayer = mySenses_.GetPlayerLatestKnownPositionAge() < 2.0f;
                if (hasRecentlySeenPlayer)
                    break;

                yield return null;
            }

            yield return null;
        }
    }
}