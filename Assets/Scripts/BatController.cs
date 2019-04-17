using System.Collections;
using UnityEngine;

public class BatController : MonoBehaviour
{
    IMovableActor movable_;
    ISensingActor senses_;
    IEnemy enemy_;
    Vector3 dir_;

    void Start()
    {
        movable_ = GetComponent<IMovableActor>();
        senses_ = GetComponent<ISensingActor>();
        senses_.LookForPlayerLoS(true, maxDistance: 10);
        enemy_ = GetComponent<IEnemy>();

        StartCoroutine(AI());
    }

    IEnumerator AI()
    {
        float baseSpeed = movable_.GetSpeed();

        while (true)
        {
            while (true)
            {
                if (enemy_.IsDead)
                    yield break;

                bool hasRecentlySeenPlayer = senses_.GetPlayerLatestKnownPositionAge() < 2.0f;
                if (!hasRecentlySeenPlayer)
                    break;

                movable_.SetSpeed(baseSpeed * 2);

                var target = senses_.GetPlayerLatestKnownPosition();
                movable_.MoveTo(target);

                yield return null;
            }

            movable_.SetSpeed(baseSpeed);

            var pos = movable_.GetPosition();
            var direction = CollisionUtil.GetRandomFreeDirection(pos) * (Random.value * 0.8f + 0.1f);
            movable_.MoveTo(pos + direction);

            float endTime = Time.time + 4 + Random.value;
            while (true)
            {
                if (movable_.MoveTargetReached())
                    break;

                if (Time.time > endTime)
                    break;

                bool hasRecentlySeenPlayer = senses_.GetPlayerLatestKnownPositionAge() < 2.0f;
                if (hasRecentlySeenPlayer)
                    break;

                yield return null;
            }

            yield return null;
        }
    }
}
