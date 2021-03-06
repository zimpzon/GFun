﻿using GFun;
using MEC;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

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
    public bool EnemyLootDisabled;
    public float EnemyMoveSpeed = 1;
    public float SpeedVariation = 0.2f;
    public int EnemyXP = 0;
    public float EnemyLife = 50;
    public int TouchPlayerDamage = 1;
    public SpriteRenderer BlipRenderer;
    public SpriteRenderer ShadowRenderer;
    public SpriteRenderer LightRenderer;

    public AudioClip DamageSound;
    public float DamageSoundPitch = 1.0f;
    public float DamageSoundPitchVariation = 0.2f;
    public AudioClip DeathSound;
    public float DeathSoundPitchVariation = 0.2f;
    public float DeathSoundPitch = 1.0f;

    public float WallAvoidancePower = 0.2f;
    public SpriteRenderer SpriteRenderer;

    public EnemyId Id => EnemyId;
    public string Name => EnemyName;
    public float Life => EnemyLife;
    public float LifePct => EnemyLife / MaxLife;
    public int Level => EnemyLevel;
    public int XP => EnemyXP;
    public bool LootDisabled => EnemyLootDisabled;

    public bool IsDead { get; set; }

    public float MaxLife { get; private set; }

    public bool HasNearbyCover { get; private set; }
    public Vector3 NearbyCoverPosition { get; private set; }
    public bool IsAttacking;

    float spriteOffsetX_;
    float speedMul_;
    float height_ = 1;
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
    Transform spriteTransform_;
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
        spriteTransform_ = SpriteRenderer.transform;
        spriteOffsetX_ = spriteTransform_.localPosition.x;
    }

    bool isFirstStart_ = true;
    private void Start()
    {
        if (aiPath_ != null)
        {
            aiPath_.destination = transform_.position;
            aiPath_.canMove = true;
        }

        if (isFirstStart_)
        {
            GameEvents.RaiseEnemySpawned(this, transform_.position);
            isFirstStart_ = false;
        }
    }

    public void EnableAiPath(bool enable)
    {
        if (aiPath_ == null)
            throw new System.InvalidOperationException("Enemy does not have an AIPath");
        aiPath_.enabled = enable;
    }

    // IShootingActor
    public void ShootAtPlayer()
    {
        nextShoot_ = Time.time + 1.0f;
    }

    float nextShoot_;
    public float ShootCdLeft => Mathf.Max(0,  Time.time - nextShoot_);

    // IPhysicsActor
    public void SetMinimumForce(Vector3 force) => throw new System.NotImplementedException();

    public void AddForce(Vector3 force, ForceMode2D forceMode = ForceMode2D.Impulse)
        => body_.AddForce(force, forceMode);

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
        {
            aiPath_.destination = destination;
            aiPath_.isStopped = false;
            aiPath_.canSearch = true;
        }
    }

    public Vector3 GetMoveDestination() => moveTo_;

    public void StopMove()
    {
        hasMoveTotarget_ = false;
        latestMovementDirection_ = Vector3.zero;
        if (aiPath_ != null)
        {
            aiPath_.isStopped = true;
            aiPath_.canSearch = false;
        }
    }

    public bool MoveTargetReached() => moveTargetReached_;

    System.Func<int, int> damageFilter_;
    public void SetDamageFilter(System.Func<int, int> filter)
        => damageFilter_ = filter;

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        if (IsDead)
        {
            body_.AddForce(damageForce * 10, ForceMode2D.Impulse);
            return;
        }

        amount += (int)(amount * CurrentRunData.Instance.DamageBonus);

        if (damageFilter_ != null)
            amount = damageFilter_(amount);

        if (amount == 0)
            return;

        DoFlash(0.5f, 0.3f);
        EnemyLife = Mathf.Max(0, EnemyLife - amount);
        if (EnemyLife == 0)
        {
            Die(damageForce);
        }
        else
        {
            AudioManager.Instance.PlaySfxClip(DamageSound, maxInstances: 3, pitchRandomVariation: 0.2f, DamageSoundPitch);
            body_.AddForce(damageForce * 5, ForceMode2D.Impulse);
        }
    }

    static WaitForSeconds DisableDelay = new WaitForSeconds(3.0f);

    void Die(Vector3 damageForce)
    {
        if (aiPath_ != null)
        {
            aiPath_.destination = transform_.position;
            aiPath_.canMove = false;
            aiPath_.enabled = false;
        }

        GameEvents.RaiseEnemyKilled(this, transform_.position);
        Timing.RunCoroutine(DieCo(damageForce).CancelWith(this.gameObject));
    }

    IEnumerator<float> DieCo(Vector3 damageForce)
    {
        DoFlash(-0.25f, 100.0f);
        IsDead = true;
        gameObject.layer = SceneGlobals.Instance.DeadEnemyLayer;
        SpriteRenderer.sortingOrder = SceneGlobals.Instance.OnTheFloorSortingValue;
        body_.freezeRotation = false;
        body_.velocity = Vector3.zero;
        body_.AddForce(damageForce * 10, ForceMode2D.Impulse);
        float angularVelocityVariation = 1.4f - Random.value * 0.8f;
        body_.angularVelocity = (damageForce.x > 0 ? -300 : 300) * damageForce.magnitude * angularVelocityVariation;
        BlipRenderer.enabled = false;
        LightRenderer.enabled = false;
        ShadowRenderer.enabled = false;

        ParticleScript.EmitAtPosition(ParticleScript.Instance.DeathFlashParticles, transform_.position, 2);
        AudioManager.Instance.PlaySfxClip(DeathSound, maxInstances: 8, DeathSoundPitchVariation, DeathSoundPitch);
        SceneGlobals.Instance.CameraShake.SetMinimumShake(0.6f);

        float endTime = Time.unscaledTime + 2.0f;
        while (Time.unscaledTime < endTime)
        {
            float velocity = body_.velocity.sqrMagnitude;
            if (velocity > 0.02f & Random.value < 0.1f)
                ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleSmokeParticles, transform_.position, 2);

            yield return 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == playerLayer_)
        {
            if (TouchPlayerDamage != 0)
            {
                var player = PlayableCharacters.GetPlayerInScene();
                player.TakeDamage(this, TouchPlayerDamage, latestMovementDirection_);
                AddForce(latestMovementDirection_ * -1);
            }
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
                aiPath_.maxSpeed = EnemyMoveSpeed * speedVariation_ * speedMul_;
                latestMovementDirection_ = aiPath_.desiredVelocity.normalized;
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
        if (IsAttacking)
        {
            StopMove();
        }

        // Some sprites (like minotaur with the big axe) should not be centered in the middle. Offset the sprite.
        spriteAnimator_?.UpdateAnimation(latestMovementDirection_, IsDead, IsAttacking);

        // If a sprite is not centered at the middle flipping x will cause if to visibly change position. Adjust for this.
        var spriteLocal = spriteTransform_.localPosition;
        spriteLocal.x = Mathf.Abs(spriteOffsetX_) * (SpriteRenderer.flipX ? -1 : 1);
        spriteTransform_.localPosition = spriteLocal;
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
        //Debug.DrawRay(cellWorld, Vector3.up * -0.2f, color, 0.3f);
        //Debug.DrawRay(cellWorld, Vector3.right * -0.2f, color, 0.3f);
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
