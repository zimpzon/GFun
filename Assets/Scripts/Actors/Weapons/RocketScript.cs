using GFun;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    [System.NonSerialized]public Vector3 Direction;

    Transform transform_;
    Vector3 position_;
    Quaternion rotation_;
    float distanceMoved_;
    Vector3 baseScale_;
    MapScript map_;
    PlainBulletSettings settings_;
    int bouncesLeft_;
    int enemyLayerMask_;

    public void Init(Vector3 position, Vector3 direction, PlainBulletSettings settings)
    {
        position_ = position;
        Direction = direction.normalized;
        settings_ = settings;
        bouncesLeft_ = settings_.MaxBounces;
        distanceMoved_ = 0;

        float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg + 180;
        rotation_ = Quaternion.Euler(0, 0, rotationDegrees);
        transform.localScale = baseScale_ * settings_.Size;

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
        if (((1 << collision.gameObject.layer) & enemyLayerMask_) != 0)
        {
            var enemyScript = collision.gameObject.GetComponent<IEnemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(settings_.Damage, Direction * settings_.DamageForce);
            }
        }
        Explode();
    }

    void Explode()
    {
        MapScript.Instance.TriggerExplosion(transform_.position, 2.5f, false, PlayerSelfDamage.Instance, true);
        SceneGlobals.Instance.RocketPool.ReturnToPool(this.gameObject);
    }

    void UpdateState()
    {
        if (distanceMoved_ > settings_.Range)
        {
            Explode();
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
            Explode();
            return;
        }

        transform_.SetPositionAndRotation(position_, rotation_);
    }

    void FixedUpdate()
    {
        UpdateState();
    }
}
