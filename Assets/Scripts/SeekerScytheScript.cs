using UnityEngine;

public class SeekerScytheScript : MonoBehaviour, IEnemy
{
    float AttractDistance = 6.0f;
    float AttractPower = 8.0f;
    float MaxVelocity = 6;
    float ChargeSoundCooldown = 4.0f;
    int TouchPlayerDamage = 1;
    public SpriteRenderer SpriteRenderer;
    public AudioClip ChargeSound;

    int playerLayer_;
    Transform rendererTransform_;
    Transform transform_;
    PlayableCharacterScript player_;
    MapScript map_;
    float sqrPickupDistance_;
    Rigidbody2D body_;
    float rotation_;
    bool isCharging_;
    float nextChargeSoundCd_;

    public EnemyId Id => EnemyId.SeekerScythe;
    public string Name => "Scythe";
    public int Level => 2;
    public float Life => 1;
    public bool IsDead => false;

    private void Awake()
    {
        transform_ = transform;
        rendererTransform_ = SpriteRenderer.transform;
        body_ = GetComponent<Rigidbody2D>();
        playerLayer_ = SceneGlobals.Instance.PlayerLayer;
    }

    public void AddForce(Vector3 force)
    {
        body_.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == playerLayer_)
        {
            var direction = body_.velocity.normalized;
            var player = PlayableCharacters.GetPlayerInScene();
            player.TakeDamage(this, TouchPlayerDamage, direction * 4);
            AddForce(direction * -1);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float time = Time.time;
        var myPos = transform_.position;

        float velocityMagnitude = body_.velocity.magnitude;
        if (velocityMagnitude > MaxVelocity)
            body_.velocity = Vector3.ClampMagnitude(body_.velocity, MaxVelocity);

        float rotationSpeed = 90 + (velocityMagnitude * 200);
        rotation_ -= dt * rotationSpeed;
        rendererTransform_.rotation = Quaternion.Euler(0, 0, rotation_);

        var playerPos = AiBlackboard.Instance.PlayerPosition;
        var diff = playerPos - myPos;
        float sqrDistance = AttractDistance * AttractDistance;
        if (diff.sqrMagnitude < sqrDistance)
        {
            if (!isCharging_ && time > nextChargeSoundCd_)
            {
                AudioManager.Instance.PlaySfxClip(ChargeSound, 1);
                nextChargeSoundCd_ = time + ChargeSoundCooldown + Random.value;
            }

            var direction = diff.normalized;
            body_.AddForce(direction * AttractPower);
            isCharging_ = true;
        }
        else
        {
            isCharging_ = false;
        }
    }

    public void DoFlash(float amount, float ms)
    {
        throw new System.NotImplementedException();
    }
}
