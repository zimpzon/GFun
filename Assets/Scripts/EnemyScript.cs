using GFun;
using System.Collections;
using UnityEngine;

// Maybe not suitable for enemies larger than 1 tile?
public class EnemyScript : MonoBehaviour, IMovableActor, ISensingActor
{
    public float Speed = 1;
    public float SpeedVariation = 0.2f;
    public float Life = 50;
    public SpriteRenderer BlipRenderer;
    public SpriteRenderer ShadowRenderer;
    public SpriteRenderer LightRenderer;
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    public float WallAvoidancePower = 0.2f;
    public SpriteRenderer SpriteRenderer;

    float speedVariation_;
    bool lookForPlayerLos_;
    float playerLoSMaxDistance_;
    float playerLatestKnownPositionTime_;
    Vector3 playerLatestKnownPosition_;
    int mapLayerMask_;
    int mapLayer_;

    bool isDead_;

    Transform transform_;
    Rigidbody2D body_;
    IMapAccess map_;
    ISpriteAnimator spriteAnimator_;
    Vector3 moveTo_;
    Vector3 wallAvoidanceDirection_;
    float wallAvoidanceAmount_;
    bool hasMoveTotarget_;
    bool moveTargetReached_;
    Vector3 latestMovementDirection_;
    float flashAmount_;
    float flashEndTime_;
    Material renderMaterial_;
    Collider2D collider_;

    void Awake()
    {
        transform_ = transform;
        body_ = GetComponent<Rigidbody2D>();
        map_ = SceneGlobals.Instance.MapAccess;
        spriteAnimator_ = GetComponentInChildren<ISpriteAnimator>();
        renderMaterial_ = SpriteRenderer.material;
        collider_ = GetComponent<Collider2D>();
        BlipRenderer.enabled = true;
        mapLayer_ = SceneGlobals.Instance.MapLayer;
        mapLayerMask_ = 1 << SceneGlobals.Instance.MapLayer;
        playerLatestKnownPosition_ = transform_.position; // Better than having last known position = 0,0
        speedVariation_ = 1.0f - ((Random.value * SpeedVariation) - SpeedVariation * 0.5f);
    }

    // ISensingActor
    public void LookForPlayerLoS(bool doCheck, float maxDistance)
    {
        lookForPlayerLos_ = doCheck;
        playerLoSMaxDistance_ = maxDistance;
    }

    public Vector3 GetPlayerLatestKnownPosition()
        => playerLatestKnownPosition_;

    public float GetPlayerLatestKnownPositionAge()
        => Time.time - playerLatestKnownPositionTime_;

    // IMovableActor
    public float GetSpeed() => Speed;
    public void SetSpeed(float speed) => Speed = speed;
    public bool IsDead() => isDead_;
    public Vector3 GetPosition() => transform_.position;

    public void MoveTo(Vector3 destination)
    {
        moveTo_ = destination;
        hasMoveTotarget_ = true;
        moveTargetReached_ = false;
    }

    public void StopMove() => hasMoveTotarget_ = false;
    public bool MoveTargetReached() => moveTargetReached_;

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        if (isDead_)
            return;
         
        DoFlash(3, 0.3f);
        Life = Mathf.Max(0, Life - amount);
        if (Life == 0)
        {
            StartCoroutine(Die(damageForce));
        }
        else
        {
            AudioManager.Instance.PlaySfxClip(DamageSound, maxInstances: 3, pitchRandomVariation: 0.3f);
            body_.AddForce(damageForce * 5, ForceMode2D.Impulse);
        }
    }

    static WaitForSeconds DisableDelay = new WaitForSeconds(3.0f);

    IEnumerator Die(Vector3 damageForce)
    {
        DoFlash(-0.25f, 100.0f);
        isDead_ = true;
        gameObject.layer = SceneGlobals.Instance.DeadEnemyLayer;
        SpriteRenderer.sortingOrder = SceneGlobals.Instance.OnTheFloorSortingValue;
        body_.freezeRotation = false;
        body_.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body_.velocity = Vector3.zero;
        body_.AddForce(damageForce * 20, ForceMode2D.Impulse);
        float angularVelocityVariation = 1.4f - Random.value * 0.8f;
        body_.angularVelocity = (damageForce.x > 0 ? -300 : 300) * damageForce.magnitude * angularVelocityVariation;
        BlipRenderer.enabled = false;
        LightRenderer.enabled = false;
        ShadowRenderer.enabled = false;

        AudioManager.Instance.PlaySfxClip(DeathSound, maxInstances: 3, pitchRandomVariation: 0.3f);
        SceneGlobals.Instance.CameraShake.SetMinimumShake(0.2f);

        yield return DisableDelay;

        body_.simulated = false;
        collider_.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == mapLayer_)
        {
            // If this event relevant anymore? Hard to tell the difference on/off.
            var targetDir = (moveTo_ - transform_.position).normalized;
            wallAvoidanceDirection_ = ((Vector3)collision.contacts[0].normal + targetDir).normalized;
            wallAvoidanceAmount_ = 1.0f + Random.value * 0.2f;
        }
    }

    void FixedUpdateInternal(float dt)
    {
        if (hasMoveTotarget_ && !moveTargetReached_)
        {
            var direction = (moveTo_ - transform_.position);
            bool targetReached = direction.sqrMagnitude < 0.05f;
            if (targetReached)
            {
                moveTargetReached_ = true;
                return;
            }

            direction.Normalize();

            latestMovementDirection_ = direction;

            var wallAvoidance = (wallAvoidanceDirection_ * wallAvoidanceAmount_ * WallAvoidancePower).normalized;
            wallAvoidanceAmount_ = Mathf.Max(0.0f, wallAvoidanceAmount_ - 4.0f * dt);

            direction = (direction + wallAvoidance).normalized;
            var step = direction * (Speed * speedVariation_) * dt;
            body_.AddForce(step * 10, ForceMode2D.Impulse);
        }
    }

    static int LosThrottleCounter = 0;
    int myLosThrottleId_ = LosThrottleCounter++;

    void DoFlash(float amount, float ms)
    {
        flashEndTime_ = Time.unscaledTime + ms;
        flashAmount_ = amount;
        renderMaterial_.SetFloat("_FlashAmount", flashAmount_);
    }

    void UpdateFlash()
    {
        if (flashAmount_ > 0.0f && Time.unscaledTime > flashEndTime_)
        {
            flashAmount_ = 0.0f;
            renderMaterial_.SetFloat("_FlashAmount", 0.0f);
        }
    }

    void Update()
    {
        UpdatePlayerLoS();
        UpdateFlash();

        spriteAnimator_.UpdateAnimation(latestMovementDirection_, isDead_);
    }

    private void FixedUpdate()
    {
        if (!isDead_)
        {
            FixedUpdateInternal(Time.fixedDeltaTime);
        }
    }

    static readonly RaycastHit2D[] MapRaycastHit = new RaycastHit2D[1];

    private void UpdatePlayerLoS()
    {
        if (isDead_)
            return;

        bool checkNow = (Time.frameCount + myLosThrottleId_) % AiBlackboard.LosThrottleModulus == 0;
        if (checkNow && !AiBlackboard.Instance.BulletTimeActive)
        {
            var playerPos = AiBlackboard.Instance.PlayerPosition;
            var myPos = transform_.position;
            myPos.x += collider_.offset.x;
            myPos.y += collider_.offset.y;

            float sqrMaxDistance = playerLoSMaxDistance_ * playerLoSMaxDistance_;
            var vec = playerPos - myPos;
            if (vec.sqrMagnitude <= sqrMaxDistance)
            {
                float distanceToPlayer = vec.magnitude;
                var direction = vec.normalized;
                int hitCount = Physics2D.RaycastNonAlloc(myPos, direction, MapRaycastHit, distanceToPlayer, mapLayerMask_);
                bool hasLoS = hitCount == 0;
                if (hasLoS)
                {
                    playerLatestKnownPosition_ = map_.GetTileBottomMid(playerPos);
                    playerLatestKnownPositionTime_ = Time.time;
                }
            }

//            Debug.DrawLine(myPos, playerLatestKnownPosition_, Color.grey);
        }
    }
}
