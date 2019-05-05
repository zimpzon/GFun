using MEC;
using System.Collections.Generic;
using UnityEngine;

public class SeekerScytheController : MonoBehaviour
{
    public float AttractDistance = 6.0f;
    public float AttractPower = 8.0f;
    public float MaxVelocity = 6;
    public float ChargeSoundCooldown = 4.0f;
    public AudioClip ChargeSound;

    Transform rendererTransform_;
    Transform transform_;
    Rigidbody2D body_;
    float rotation_;
    bool isCharging_;
    float nextChargeSoundCd_;

    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;
    EnemyScript enemyScript_;
    Vector3 dir_;

    private void Start()
    {
        enemyScript_ = GetComponent<EnemyScript>();
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();
        rendererTransform_ = enemyScript_.SpriteRenderer.transform;
        transform_ = transform;
        body_ = GetComponent<Rigidbody2D>();

        Timing.RunCoroutine(AICo().CancelWith(gameObject));
    }

    IEnumerator<float> AICo()
    {
        while (true)
        {
            if (me_.IsDead)
                yield break;

            float dt = Time.deltaTime;
            float time = Time.time;
            var myPos = transform_.position;

            float velocityMagnitude = body_.velocity.magnitude;
            if (velocityMagnitude > MaxVelocity)
                body_.velocity = Vector3.ClampMagnitude(body_.velocity, MaxVelocity);

            float rotationSpeed = 90 + (velocityMagnitude * 200);
            rotation_ -= dt * rotationSpeed;
            rendererTransform_.rotation = Quaternion.Euler(0, 0, rotation_);

            var playerPos = AiBlackboard.Instance.PlayerPosition;
            var diff = playerPos - myPos;
            float sqrDistance = AttractDistance * AttractDistance;
            if (diff.sqrMagnitude < sqrDistance)
            {
                if (!isCharging_ && time > nextChargeSoundCd_)
                {
                    AudioManager.Instance.PlaySfxClip(ChargeSound, 1);
                    nextChargeSoundCd_ = time + ChargeSoundCooldown + Random.value;
                }

                var direction = diff.normalized;
                myPhysics_.AddForce(direction * AttractPower, ForceMode2D.Force);
                isCharging_ = true;
            }
            else
            {
                isCharging_ = false;
            }

            yield return 0;
        }
    }
}
