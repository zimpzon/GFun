using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public Vector3 Direction;
    public float Speed;
    public float MaxRange;

    Transform transform_;
    Vector3 position_;
    Quaternion rotation_;
    float range_;

    // TODO: Object pooling
    public void Init(Vector3 position, Vector3 direction, float speed, float maxRange)
    {
        position_ = position;
        Direction = direction;
        MaxRange = maxRange;

        Speed = speed;
        float rotationDegrees = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        rotation_ = Quaternion.Euler(0, 0, rotationDegrees);

        UpdateState();
    }

    private void Awake()
    {
        transform_ = transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DieParticles();
        Destroy(this.gameObject);
    }

    void DieParticles()
    {
        var particles = SceneGlobals.Instance.ParticleScript.BulletFizzleParticles;
        particles.transform.position = position_;
        particles.Emit(4);
    }

    void UpdateState()
    {
        if (range_ >= MaxRange)
        {
            DieParticles();
            Destroy(this.gameObject);
        }

        float distance = Speed * Time.fixedDeltaTime;
        position_ += Direction * distance;
        range_ += distance;

        transform_.SetPositionAndRotation(position_, rotation_);
    }

    void FixedUpdate()
    {
        UpdateState();
    }
}
