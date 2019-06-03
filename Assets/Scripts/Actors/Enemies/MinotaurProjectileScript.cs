using UnityEngine;

public class MinotaurProjectileScript : MonoBehaviour
{
    public Vector3 Direction;
    public float RotationOffset;

    Transform transform_;
    Vector3 position_;
    float distanceMoved_;
    Vector3 baseScale_;
    LayerMask playerLayer_;
    MapScript map_;
    float range_;
    float speed_;
    float turnSpeed_;
    int damage_;
    IEnemy owner_;
    bool collideWalls_;

    public void Init(IEnemy owner, Vector3 position, Vector3 direction, float range, float speed, float turnSpeed, int damage, bool collideWalls = true)
    {
        owner_ = owner;
        position_ = position;
        Direction = direction.normalized;
        distanceMoved_ = 0;
        range_ = range;
        speed_ = speed;
        turnSpeed_ = turnSpeed;
        damage_ = damage;
        collideWalls_ = collideWalls;

        UpdateState();
    }

    private void Awake()
    {
        transform_ = transform;
        baseScale_ = transform.localScale;
        playerLayer_ = SceneGlobals.Instance.PlayerLayer;
        map_ = SceneGlobals.Instance.MapScript;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer_.value)
        {
            var player = collision.gameObject.GetComponent<PlayableCharacterScript>();
            player.TakeDamage(owner_, damage_, Direction);
            Die();
        }
    }

    void Die()
    {
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.BulletFizzleParticles, position_, 10);
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleFlashParticles, position_, 1);
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.MuzzleSmokeParticles, position_, 10);
        SceneGlobals.Instance.EnemyBullet1Pool.ReturnToPool(this.gameObject);
    }

    void UpdateState()
    {
        if (distanceMoved_ > range_)
        {
            Die();
            return;
        }

        float distance = speed_ * Time.fixedDeltaTime;

        // Move X
        Vector3 newPosX = position_;
        newPosX.x += Direction.x * distance;
        if (map_.GetCollisionTileValue(newPosX) != MapBuilder.TileWalkable)
            Direction.x *= -1;

        // Move Y
        Vector3 newPosY = position_;
        newPosY.y += Direction.y * distance;
        if (map_.GetCollisionTileValue(newPosY) != MapBuilder.TileWalkable)
            Direction.y *= -1;

        var directionToPlayer = (AiBlackboard.Instance.PlayerPosition - position_).normalized;
        Direction = Vector3.RotateTowards(Direction, directionToPlayer, Time.deltaTime * turnSpeed_, 1.0f);

        float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        transform_.rotation = Quaternion.Euler(0, 0, rotationDegrees + RotationOffset);

        position_ += Direction * distance;
        distanceMoved_ += distance;

        if (collideWalls_ && map_.GetCollisionTileValue(position_) != MapBuilder.TileWalkable)
        {
            Die();
            return;
        }

        transform_.position = position_;
    }

    void FixedUpdate()
    {
        UpdateState();
    }
}
