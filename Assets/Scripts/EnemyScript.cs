using GFun;
using System.Collections;
using UnityEngine;

public class EnemyScript : MonoBehaviour, IMovableActor, ISensingActor
{
    public float Speed = 1;
    public float Life = 50;
    public float Drag = 1.0f;
    public bool IsDead = false;
    public SpriteRenderer BlipRenderer;
    public SpriteRenderer ShadowRenderer;
    public SpriteRenderer LightRenderer;
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    public float WallAvoidancePower = 0.2f;

    bool lookForPlayerLos_;
    float playerLoSMaxDistance_;
    float playerLatestKnownPositionTime_;
    Vector3 playerLatestKnownPosition_;
    int mapLayerMask_;
    int mapLayer_;

    Transform transform_;
    Rigidbody2D body_;
    IMapAccess map_;
    ISpriteAnimator spriteAnimator_;
    Vector3 force_;
    Vector3 moveTo_;
    Vector3 wallAvoidanceDirection_;
    float wallAvoidanceAmount_;
    bool hasMoveTotarget_;
    bool moveTargetReached_;
    Vector3 latestMovenentDirection_;
    float flashAmount_;
    float flashEndTime_;
    Material renderMaterial_;
    SpriteRenderer renderer_;
    Collider2D collider_;

    void Awake()
    {
        transform_ = transform;
        body_ = GetComponent<Rigidbody2D>();
        map_ = SceneGlobals.Instance.MapAccess;
        spriteAnimator_ = GetComponentInChildren<ISpriteAnimator>();
        renderer_ = GetComponentInChildren<SpriteRenderer>();
        renderMaterial_ = renderer_.material;
        collider_ = GetComponent<Collider2D>();
        BlipRenderer.enabled = true;
        mapLayer_ = SceneGlobals.Instance.MapLayer;
        mapLayerMask_ = 1 << SceneGlobals.Instance.MapLayer;
        playerLatestKnownPosition_ = transform_.position; // Better than having last known position = 0,0
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
    public Vector3 GetPosition()
        => transform_.position;

    public void MoveTo(Vector3 destination)
    {
        moveTo_ = destination;
        hasMoveTotarget_ = true;
        moveTargetReached_ = false;
    }

    public void StopMove()
        => hasMoveTotarget_ = false;

    public bool MoveTargetReached()
        => moveTargetReached_;

    public void SetMinimumForce(Vector3 force)
    {
        if (force.sqrMagnitude > force_.sqrMagnitude)
            force_ = force;
    }

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        DoFlash(3, 0.3f);
        Life = Mathf.Max(0, Life - amount);
        if (Life == 0)
        {
            StartCoroutine(Die(damageForce));
        }
        else
        {
            AudioManager.Instance.PlaySfxClip(DamageSound, maxInstances: 3, pitchRandomVariation: 0.3f);
            SetForce(damageForce * 10 / body_.mass);
        }
    }

    static WaitForSeconds DisableDelay = new WaitForSeconds(3.0f);

    IEnumerator Die(Vector3 damageForce)
    {
        DoFlash(-0.25f, 100.0f);
        IsDead = true;
        gameObject.layer = SceneGlobals.Instance.DeadEnemyLayer;
        renderer_.sortingOrder = SceneGlobals.Instance.OnTheFloorSortingValue;
        body_.freezeRotation = false;
        body_.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body_.AddForce(damageForce * 2000);
        float angularVelocityVariation = 1.4f - Random.value * 0.8f;
        body_.angularVelocity = (force_.x > 0 ? -100 : 100) * damageForce.magnitude * angularVelocityVariation;
        BlipRenderer.enabled = false;
        LightRenderer.enabled = false;
        ShadowRenderer.enabled = false;

        AudioManager.Instance.PlaySfxClip(DeathSound, maxInstances: 3, pitchRandomVariation: 0.3f);
        SceneGlobals.Instance.CameraShake.SetMinimumShake(0.2f);

        yield return DisableDelay;

        body_.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        body_.isKinematic = true;
        collider_.enabled = false;
    }

    public void SetForce(Vector3 force)
    {
        force_ = force;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == mapLayer_)
        {
            wallAvoidanceDirection_ = ((Vector3)collision.contacts[0].normal + latestMovenentDirection_).normalized;
            wallAvoidanceAmount_ = 1.0f;
        }
    }

    void FixedUpdateInternal(float dt)
    {
        if (hasMoveTotarget_ && !moveTargetReached_)
        {
            var direction = (moveTo_ - transform_.position);
            bool targetReached = direction.sqrMagnitude < 0.1f;
            if (targetReached)
                moveTargetReached_ = true;

            direction.Normalize();

            latestMovenentDirection_ = direction;

            var wallAvoidance = (wallAvoidanceDirection_ * wallAvoidanceAmount_ * WallAvoidancePower).normalized;
            wallAvoidanceAmount_ = Mathf.Max(0.0f, wallAvoidanceAmount_ - 4.0f * dt);

            direction = (direction + wallAvoidance).normalized; 
            var step = direction * Speed * dt + force_;
            body_.MovePosition(transform_.position + step);
        }

        if (force_.sqrMagnitude > 0.0f)
        {
            float forceLen = force_.magnitude;
            forceLen = Mathf.Clamp(forceLen - Drag * dt, 0.0f, float.MaxValue);
            force_ = force_.normalized * forceLen;
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
        if (!IsDead)
        {
            UpdatePlayerLoS();
        }

        UpdateFlash();
        spriteAnimator_.UpdateAnimation(latestMovenentDirection_, IsDead);
    }

    private void FixedUpdate()
    {
        if (!IsDead)
        {
            FixedUpdateInternal(Time.fixedDeltaTime);
        }
    }

    static readonly RaycastHit2D[] MapRaycastHit = new RaycastHit2D[1];

    private void UpdatePlayerLoS()
    {
        bool checkNow = (Time.frameCount + myLosThrottleId_) % AiBlackboard.LosThrottleModulus == 0;
        checkNow = true;
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
                    playerLatestKnownPosition_ = playerPos;
                    playerLatestKnownPositionTime_ = Time.time;
                }

            }
            Debug.DrawLine(myPos, playerLatestKnownPosition_, Color.grey);
        }
    }
}
