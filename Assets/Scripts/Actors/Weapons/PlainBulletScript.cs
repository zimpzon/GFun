using GFun;
using UnityEngine;

public class PlainBulletScript : MonoBehaviour
{
    public Vector3 Direction;

    Transform transform_;
    Vector3 position_;
    Quaternion rotation_;
    float distanceMoved_;
    Vector3 baseScale_;
    LayerMask enemyLayer_;
    PlainBulletSettings settings_;
    MapScript map_;
    int remainingDamage_;

    public void Init(Vector3 position, Vector3 direction, PlainBulletSettings settings)
    {
        position_ = position;
        Direction = direction.normalized;
        settings_ = settings;
        distanceMoved_ = 0;
        remainingDamage_ = settings.Damage;

        float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        rotation_ = Quaternion.Euler(0, 0, rotationDegrees);
        transform.localScale = baseScale_ * settings.Size;

        UpdateState();
    }

    private void Awake()
    {
        transform_ = transform;
        baseScale_ = transform.localScale;
        enemyLayer_ = SceneGlobals.Instance.EnemyLayer;
        map_ = SceneGlobals.Instance.MapScript;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == enemyLayer_.value && remainingDamage_ > 0)
        {
            var enemyScript = collision.gameObject.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(settings_.Damage, Direction * settings_.DamageForce);
                remainingDamage_ = 0;
            }
        }

        Die();
    }

    void Die()
    {
        ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.BulletFizzleParticles, position_, 4);
        SceneGlobals.Instance.ElongatedBulletPool.ReturnToPool(this.gameObject);
    }

    void UpdateState()
    {
        if (distanceMoved_ > settings_.Range)
        {
            Die();
            return;
        }

        float distance = settings_.Speed * Time.fixedDeltaTime;
        position_ += Direction * distance;
        distanceMoved_ += distance;
        if (map_.GetCollisionTileValue(position_) != MapBuilder.TileWalkable)
        {
            Die();
            return;
        }

        transform_.SetPositionAndRotation(position_, rotation_);
    }

    void FixedUpdate()
    {
        UpdateState();
    }
}
