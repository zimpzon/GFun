using GFun;
using System.Collections;
using UnityEngine;

// Maybe not suitable for enemies larger than 1 tile?
public class EnemyScript : MonoBehaviour, IMovableActor, ISensingActor, IEnemy, IPhysicsActor
{
    [Header("Debug")]
    public bool EnableMoveToGizmo;
    public Color MoveToGizmoColor = Color.yellow;
    public bool EnableCoverGizmo;
    public Color CoverGizmoColor = Color.cyan;
    public bool EnablePlayerLoSGizmo;
    public Color PlayerLOSGizmoColor = Color.green;
    Vector3 latestLOSStart_;
    Vector3 latestLOSEnd_;
    bool latestLOSFoundPlayer_;

    [Header("Enemy")]
    public EnemyId EnemyId;
    public string EnemyName;
    public int EnemyLevel = 1;
    public float EnemyMoveSpeed = 1;
    public float SpeedVariation = 0.2f;
    public float EnemyLife = 50;
    public int TouchPlayerDamage = 1;
    public SpriteRenderer BlipRenderer;
    public SpriteRenderer ShadowRenderer;
    public SpriteRenderer LightRenderer;
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    public float WallAvoidancePower = 0.2f;
    public SpriteRenderer SpriteRenderer;

    public EnemyId Id => EnemyId;
    public string Name => EnemyName;
    public float Life => EnemyLife;
    public int Level => EnemyLevel;

    public bool IsDead { get; set; }

    public float MaxLife { get; private set; }

    public bool HasNearbyCover { get; private set; }
    public Vector3 NearbyCoverPosition { get; private set; }

    float width_ = 1;
    float height_ = 1;
    float life_;
    float speedVariation_;
    bool lookForPlayerLos_;
    float playerLoSMaxDistance_;
    float playerLatestKnownPositionTime_ = float.MinValue;
    Vector3 playerLatestKnownPosition_;
    bool lookForNearbyCover_;
    float coverMaxDistance_;
    int mapLayerMask_;
    int mapLayer_;
    int playerLayer_;

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

    private void OnDrawGizmos()
    {
        if (EnableMoveToGizmo)
        {
            if (hasMoveTotarget_)
            {
                Gizmos.color = MoveToGizmoColor;
                Gizmos.DrawLine(transform_.position, moveTo_);
                Gizmos.DrawWireSphere(moveTo_, 0.2f);
            }
        }

        if (EnablePlayerLoSGizmo)
        {
            Gizmos.color = latestLOSFoundPlayer_ ? Color.green : Color.grey;
            Gizmos.DrawLine(latestLOSStart_, latestLOSEnd_);

            Gizmos.color = PlayerLOSGizmoColor;
            Gizmos.DrawWireSphere(playerLatestKnownPosition_, 0.15f);
        }

        if (EnableCoverGizmo && HasNearbyCover)
        {
            Gizmos.color = CoverGizmoColor;
            Gizmos.DrawWireSphere(NearbyCoverPosition, 0.1f);
        }
    }

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
        playerLayer_ = SceneGlobals.Instance.PlayerLayer;
        speedVariation_ = 1.0f - ((Random.value * SpeedVariation) - SpeedVariation * 0.5f);
        MaxLife = EnemyLife;

        SetLookForNearbyCover(true, 3);
    }

    // IPhysicsActor
    public void SetMinimumForce(Vector3 force) => throw new System.NotImplementedException();

    public void SetForce(Vector3 force) => throw new System.NotImplementedException();

    public void AddForce(Vector3 force)
        => body_.AddForce(force, ForceMode2D.Impulse);

    // ISensingActor
    public void SetLookForPlayerLoS(bool doCheck, float maxDistance)
    {
        lookForPlayerLos_ = doCheck;
        playerLoSMaxDistance_ = maxDistance;
    }

    public void SetLookForNearbyCover(bool doCheck, float maxDistance)
    {
        lookForNearbyCover_ = doCheck;
        coverMaxDistance_ = maxDistance;
    }

    public Vector3 GetPlayerLatestKnownPosition(PlayerPositionType type)
    {
        if (type == PlayerPositionType.Feet)
            return playerLatestKnownPosition_;
        else if (type == PlayerPositionType.Center)
            return playerLatestKnownPosition_ + Vector3.up * 0.5f;
        else if (type == PlayerPositionType.Tile)
            return map_.GetTileBottomMid(playerLatestKnownPosition_);

        throw new System.ArgumentException($"Unknown position type: {type}");
    }

    public float GetPlayerLatestKnownPositionAge()
        => Time.time - playerLatestKnownPositionTime_;

    // IMovableActor
    public float GetSpeed() => EnemyMoveSpeed;
    public void SetSpeed(float speed) => EnemyMoveSpeed = speed;
    public Vector3 GetPosition() => transform_.position;
    public Vector3 GetCenter() => transform_.position + transform_.rotation * (Vector3.up * height_ * 0.5f);

    public void MoveTo(Vector3 destination)
    {
        if (IsDead)
            return;

        moveTo_ = destination;
        hasMoveTotarget_ = true;
        moveTargetReached_ = false;
    }

    public Vector3 GetMoveDestination() => moveTo_;

    public void StopMove() => hasMoveTotarget_ = false;
    public bool MoveTargetReached() => moveTargetReached_;

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        if (IsDead)
            return;
         
        DoFlash(3, 0.3f);
        EnemyLife = Mathf.Max(0, EnemyLife - amount);
        if (EnemyLife == 0)
        {
            Die(damageForce);
        }
        else
        {
            AudioManager.Instance.PlaySfxClip(DamageSound, maxInstances: 3, pitchRandomVariation: 0.3f);
            body_.AddForce(damageForce * 5, ForceMode2D.Impulse);
        }
    }

    static WaitForSeconds DisableDelay = new WaitForSeconds(3.0f);

    void Die(Vector3 damageForce)
    {
        GameEvents.RaiseEnemyKilled(this, transform_.position);
        StartCoroutine(DieCo(damageForce));
    }

    IEnumerator DieCo(Vector3 damageForce)
    {
        DoFlash(-0.25f, 100.0f);
        IsDead = true;
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
        if (collision.gameObject.layer == playerLayer_)
        {
            var player = PlayableCharacters.GetPlayerInScene();
            player.TakeDamage(this, TouchPlayerDamage, latestMovementDirection_);
            AddForce(latestMovementDirection_ * -1);
        }

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
            wallAvoidanceAmount_ = Mathf.Max(0.0f, wallAvoidanceAmount_ - (4.0f * dt) * WallAvoidancePower);

            direction = (direction + wallAvoidance).normalized;
            var step = direction * (EnemyMoveSpeed * speedVariation_) * dt;
            body_.AddForce(step * 10, ForceMode2D.Impulse);
        }
    }

    static int LosThrottleCounter = 0;
    int myLosThrottleId_ = LosThrottleCounter++;

    public void DoFlash(float amount, float ms)
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
        UpdateNearbyCover();
        UpdateFlash();

        spriteAnimator_.UpdateAnimation(latestMovementDirection_, IsDead);
    }

    private void FixedUpdate()
    {
        if (!IsDead)
        {
            FixedUpdateInternal(Time.fixedDeltaTime);
        }
    }

    static readonly RaycastHit2D[] MapRaycastHit = new RaycastHit2D[1];

    bool HasCoverBehindWall(int wallX, int wallY, Vector3 fromPosition, out Vector3 coverPosition)
    {
        Vector3 cellWorld = new Vector3(wallX + 0.5f, wallY + 0.5f);
        Vector3 dir = (cellWorld - fromPosition).normalized;
        Vector3 testPos = cellWorld + dir;
        int testCellX = (int)testPos.x;
        int testCellY = (int)testPos.y;
        bool isFree = MapBuilder.CollisionMap[testCellX, testCellY] == MapBuilder.TileWalkable;
        var color = isFree ? Color.green : Color.red;
        var freeCellWorld = new Vector3(testCellX + 0.5f, testCellY + 0.5f, 0);
        coverPosition = freeCellWorld;
        //Debug.DrawRay(freeCellWorld, Vector3.up * 0.5f, color, 0.3f);
        //Debug.DrawRay(freeCellWorld, Vector3.right * 0.5f, color, 0.3f);
        return isFree;
    }

    void UpdateNearbyCover()
    {
        if (IsDead || !lookForNearbyCover_)
            return;

        bool checkNow = (Time.frameCount + myLosThrottleId_) % AiBlackboard.LosThrottleModulus == 0;
        if (checkNow && !AiBlackboard.Instance.BulletTimeActive)
        {
            // If player is last seen above or below us we scan for cover to the left and right.
            // If player is last seen left or right of us we scan for cover up and down.
            var myPos = transform_.position;
            myPos.x += collider_.offset.x;
            myPos.y += collider_.offset.y;

            HasNearbyCover = false;
            bool hasRecentlySeenPlayer = Time.time - playerLatestKnownPositionTime_ < 3.0f;
            if (!hasRecentlySeenPlayer) // Don't look for cover if player wasn't recently seen
                return;

            var playerLOSDir = playerLatestKnownPosition_ - myPos;
            float sqrMaxDistance = playerLoSMaxDistance_ * playerLoSMaxDistance_;
            if (playerLOSDir.sqrMagnitude > sqrMaxDistance)
                return;

            int startX = (int)myPos.x;
            int startY = (int)myPos.y;

            int cellRange = (int)coverMaxDistance_;
            for (int y = startY - cellRange; y <= startY + cellRange; ++y)
            {
                if (HasNearbyCover)
                    break;
                for (int x = startX - cellRange; x <= startX + cellRange; ++x)
                {
                    if (MapBuilder.CollisionMap[x, y] != MapBuilder.TileWalkable)
                    {
                        if (HasCoverBehindWall(x, y, playerLatestKnownPosition_, out Vector3 coverPosition))
                        {
                            var cellWorld = new Vector3(x + 0.5f, x + 0.5f, 0);
                            int hitCount = Physics2D.LinecastNonAlloc(myPos, coverPosition, MapRaycastHit, mapLayerMask_);
                            if (hitCount == 0)
                            {
                                HasNearbyCover = true;
                                NearbyCoverPosition = coverPosition;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdatePlayerLoS()
    {
        if (IsDead || !lookForPlayerLos_)
            return;

        bool checkNow = (Time.frameCount + myLosThrottleId_) % AiBlackboard.LosThrottleModulus == 0;
        if (checkNow && !AiBlackboard.Instance.BulletTimeActive)
        {
            var playerPos = AiBlackboard.Instance.PlayerPosition;
            var myPos = transform_.position;
            myPos.x += collider_.offset.x;
            myPos.y += collider_.offset.y;
            latestLOSStart_ = myPos;

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
                    latestLOSEnd_ = playerPos;
                    latestLOSFoundPlayer_ = true;
                }
                else
                {
                    latestLOSEnd_ = MapRaycastHit[0].point;
                    latestLOSFoundPlayer_ = false;
                }
            }
            else
            {
                // Out of range
                latestLOSEnd_ = myPos + vec.normalized * playerLoSMaxDistance_;
                latestLOSFoundPlayer_ = false;
            }
        }
    }
}
