using GFun;
using UnityEngine;

public class EnemyBullet1Script : MonoBehaviour
{
    public Vector3 Direction;

    Transform transform_;
    Vector3 position_;
    float distanceMoved_;
    Vector3 baseScale_;
    LayerMask playerLayer_;
    MapScript map_;
    float range_;
    float speed_;
    int damage_;
    IEnemy owner_;

    public void Init(IEnemy owner, Vector3 position, Vector3 direction, float range, float speed, int damage)
    {
        owner_ = owner;
        position_ = position;
        Direction = direction.normalized;
        distanceMoved_ = 0;
        range_ = range;
        speed_ = speed;
        damage_ = damage;

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
        }

        Die();
    }

    void Die()
    {
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.BulletFizzleParticles, position_, 4);
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
        position_ += Direction * distance;
        distanceMoved_ += distance;
        if (map_.GetCollisionTileValue(position_) != MapBuilder.TileWalkable)
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
