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
    int enemyLayerMask_;
    PlainBulletSettings settings_;
    MapScript map_;
    int remainingDamage_;
    int bouncesLeft_;

    public void Init(Vector3 position, Vector3 direction, PlainBulletSettings settings)
    {
        position_ = position;
        Direction = direction.normalized;
        settings_ = settings;
        distanceMoved_ = 0;
        remainingDamage_ = settings.Damage;
        bouncesLeft_ = settings.MaxBounces;

        float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg + 180;
        rotation_ = Quaternion.Euler(0, 0, rotationDegrees);
        transform.localScale = baseScale_ * settings.Size;

        UpdateState();
    }

    private void Awake()
    {
        transform_ = transform;
        baseScale_ = transform.localScale;
        enemyLayerMask_ = SceneGlobals.Instance.EnemyAliveMask;
        map_ = SceneGlobals.Instance.MapScript;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayerMask_) != 0 && remainingDamage_ > 0)
        {
            var enemyScript = collision.gameObject.GetComponent<IEnemy>();
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

        if (bouncesLeft_ > 0)
        {
            bool updateAngle = false;

            // Move X
            Vector3 newPosX = position_;
            newPosX.x += Direction.x * distance;
            if (map_.GetCollisionTileValue(newPosX) != MapBuilder.TileWalkable)
            {
                Direction.x *= -1;
                updateAngle = true;
            }

            // Move Y
            Vector3 newPosY = position_;
            newPosY.y += Direction.y * distance;
            if (map_.GetCollisionTileValue(newPosY) != MapBuilder.TileWalkable)
            {
                Direction.y *= -1;
                updateAngle = true;
            }

            if (updateAngle)
            {
                float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg + 180;
                rotation_ = Quaternion.Euler(0, 0, rotationDegrees);
                bouncesLeft_--;
            }
        }

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
