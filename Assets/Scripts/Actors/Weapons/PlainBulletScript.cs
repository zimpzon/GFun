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

    PlainBulletSettings settings_;

    public void Init(Vector3 position, Vector3 direction, PlainBulletSettings settings)
    {
        position_ = position;
        Direction = direction;
        settings_ = settings;
        distanceMoved_ = 0;

        float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        rotation_ = Quaternion.Euler(0, 0, rotationDegrees);
        transform.localScale = baseScale_ * settings.Size;

        UpdateState();
    }

    private void Awake()
    {
        transform_ = transform;
        baseScale_ = transform.localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Die();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
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

        transform_.SetPositionAndRotation(position_, rotation_);
    }

    void FixedUpdate()
    {
        UpdateState();
    }
}
