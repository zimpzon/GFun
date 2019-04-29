using GFun;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Maybe not suitable for enemies larger than 1 tile?
public class EnemyScript : MonoBehaviour, IMovableActor, ISensingActor, IEnemy, IPhysicsActor, IShootingActor
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

    float speedMul_;
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
    AIPath aiPath_;

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
        aiPath_ = GetComponent<AIPath>();

        BlipRenderer.enabled = true;
        mapLayer_ = SceneGlobals.Instance.MapLayer;
        mapLayerMask_ = 1 << SceneGlobals.Instance.MapLayer;
        playerLayer_ = SceneGlobals.Instance.PlayerLayer;
        speedVariation_ = 1.0f - ((Random.value * SpeedVariation) - SpeedVariation * 0.5f);
        MaxLife = EnemyLife;
    }

    private void Start()
    {
        if (aiPath_ != null)
        {
            aiPath_.destination = transform_.position;
            aiPath_.canMove = true;
        }
    }

    // IShootingActor
    public void ShootAtPlayer()
    {
        FloatingTextSpawner.Instance.Spawn(transform_.position, "Bang!");
        nextShoot_ = Time.time + 1.0f;
    }

    float nextShoot_;
    public float ShootCdLeft => Mathf.Max(0,  Time.time - nextShoot_);

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

    public void MoveTo(Vector3 destination, float speedMul = 1.0f)
    {
        if (IsDead)
            return;

        speedMul_ = speedMul;
        moveTo_ = destination;
        hasMoveTotarget_ = true;
        moveTargetReached_ = false;

        if (aiPath_ != null)
            aiPath_.destination = destination;
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
        if (aiPath_ != null)
            aiPath_.canMove = false;

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

    void MoveStraightToTarget(float dt)
    {
        var direction = (moveTo_ - transform_.position);
        bool targetReached = direction.sqrMagnitude < 0.005f;
        if (targetReached)
        {
            moveTargetReached_ = true;
            body_.velocity = Vector2.zero;
            return;
        }

        direction.Normalize();

        latestMovementDirection_ = direction;

        var wallAvoidance = (wallAvoidanceDirection_ * wallAvoidanceAmount_ * WallAvoidancePower).normalized;
        wallAvoidanceAmount_ = Mathf.Max(0.0f, wallAvoidanceAmount_ - (4.0f * dt) * WallAvoidancePower);

        direction = (direction + wallAvoidance).normalized;
        var step = direction * (EnemyMoveSpeed * speedVariation_ * speedMul_) * dt;
        body_.AddForce(step * 10, ForceMode2D.Impulse);
    }

    void FixedUpdateInternal(float dt)
    {
        if (hasMoveTotarget_ && !moveTargetReached_)
        {
            if (aiPath_ == null)
            {
                MoveStraightToTarget(dt);
            }
            else
            {
                moveTargetReached_ = aiPath_.reachedEndOfPath;
            }
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

    bool IsCover(int wallX, int wallY, Vector3 fromPosition)
    {
        Vector3 cellWorld = new Vector3(wallX + 0.5f, wallY + 0.5f);
        int count = Physics2D.LinecastNonAlloc(cellWorld, fromPosition, MapRaycastHit, mapLayerMask_);
        bool isCover = count != 0;
        var color = isCover ? Color.cyan : Color.magenta;
        Debug.DrawRay(cellWorld, Vector3.up * -0.2f, color, 0.3f);
        Debug.DrawRay(cellWorld, Vector3.right * -0.2f, color, 0.3f);
        return isCover;
    }

    bool IsPotentialCover(int x, int y, Vector3 fromPosition)
    {
        if (MapBuilder.CollisionMap[x, y] != 0)
            return false;

        int neighbourCount =
            MapBuilder.CollisionMap[x + 1, y + 0] +
            MapBuilder.CollisionMap[x + 0, y + 1] +
            MapBuilder.CollisionMap[x - 1, y + 0] +
            MapBuilder.CollisionMap[x + 0, y - 1];

        if (neighbourCount == 0 || neighbourCount == 4)
            return false;

        int lookX = fromPosition.x > x + 0.5f ? 1 : -1;
        int lookY = fromPosition.y > y + 0.5f ? 1 : -1;
        int candidateWallCount = MapBuilder.CollisionMap[x + lookX, y] + MapBuilder.CollisionMap[x, y + lookY];
        return candidateWallCount != 0;
    }

    struct CoverNode
    {
        public int x;
        public int y;
        public int distance;
    };

    Stack<CoverNode> floodFillStack_ = new Stack<CoverNode>(50);
    List<CoverNode> reachableNodes_ = new List<CoverNode>(50);
    List<int> distinctCheck_ = new List<int>(50);

    void FloodNode(int x, int y, int startX, int startY, int radius)
    {
        int distanceX = Mathf.Abs(startX - x);
        int distanceY = Mathf.Abs(startY - y);
        if (distanceX > radius || distanceY > radius)
            return;

        if (MapBuilder.CollisionMap[x, y] != 0)
            return;

        int packed = x + (y << 16);
        if (!distinctCheck_.Contains(packed))
        {
            int distance = distanceX + distanceY;
            var node = new CoverNode { x = x, y = y, distance = distance };
            reachableNodes_.Add(node);
            floodFillStack_.Push(node);
            distinctCheck_.Add(packed);
        }
    }

    void FloodFill(int startX, int startY, int radius)
    {
        floodFillStack_.Clear();
        reachableNodes_.Clear();
        distinctCheck_.Clear();

        FloodNode(startX, startY, startX, startY, radius);

        while (floodFillStack_.Count > 0)
        {
            var node = floodFillStack_.Pop();
            int x = node.x;
            int y = node.y;
            FloodNode(x - 1, y + 0, startX, startY, radius);
            FloodNode(x + 0, y - 1, startX, startY, radius);
            FloodNode(x + 1, y + 0, startX, startY, radius);
            FloodNode(x + 0, y + 1, startX, startY, radius);
        }
    }

    void UpdateNearbyCover()
    {
        if (IsDead || !lookForNearbyCover_)
            return;

        bool checkNow = (Time.frameCount + myLosThrottleId_) % AiBlackboard.LosThrottleModulus == 0;
        if (checkNow && !AiBlackboard.Instance.BulletTimeActive)
        {
            var myPos = transform_.position;
            myPos.x += collider_.offset.x;
            myPos.y += collider_.offset.y;

            HasNearbyCover = false;

            var playerLOSDir = playerLatestKnownPosition_ - myPos;
            float sqrMaxDistance = playerLoSMaxDistance_ * playerLoSMaxDistance_;
            if (playerLOSDir.sqrMagnitude > sqrMaxDistance)
                return;

            int startX = (int)myPos.x;
            int startY = (int)myPos.y;
            int cellRange = (int)coverMaxDistance_;
            FloodFill(startX, startY, cellRange);

            int idxClosestCover = -1;
            int smallestDistance = int.MaxValue;

            for (int i = 0; i < reachableNodes_.Count; ++i)
            {
                var node = reachableNodes_[i];
                int x = node.x;
                int y = node.y;
                Vector3 cellWorld = new Vector3(x + 0.5f, y + 0.25f);

                bool isPotentialCover = IsPotentialCover(x, y, AiBlackboard.Instance.PlayerPosition);
                if (isPotentialCover)
                {
                    Color color = isPotentialCover ? Color.cyan : Color.magenta;
                    //Debug.DrawRay(cellWorld, Vector3.up * 0.5f, color, 0.3f);
                    //Debug.DrawRay(cellWorld, Vector3.right * 0.5f, color, 0.3f);
                    bool isCover = IsCover(x, y, AiBlackboard.Instance.PlayerPosition);
                    if (isCover)
                    {
                        if (node.distance < smallestDistance)
                        {
                            idxClosestCover = i;
                            smallestDistance = node.distance;
                        }
                    }
                }
            }

            if (idxClosestCover != -1)
            {
                var node = reachableNodes_[idxClosestCover];
                NearbyCoverPosition = new Vector3(node.x + 0.5f, node.y + 0.25f);
                HasNearbyCover = true;
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
