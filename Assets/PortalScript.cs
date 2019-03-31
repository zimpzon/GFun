using UnityEngine;
using UnityEngine.Events;

public class PortalScript : MonoBehaviour
{
    public Transform EntryPoint;
    public SpriteAnimationFrames_Single Anim;
    public float FloatDistance = 0.1f;
    public float FloatSpeed = 0.2f;
    public float PullForce = 10;
    public UnityEvent OnPlayerEnter;

    SpriteRenderer renderer_;
    Transform transform_;
    Vector3 basePosition_;
    CircleCollider2D collider_;
    Vector3 entryPoint_;
    PlayerScript playerScript_;
    LayerMask playerLayer_;
    float colliderRadius_;
    CameraShake cameraShake_;
    bool enterTriggered_;

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        transform_ = transform;
        collider_ = GetComponent<CircleCollider2D>();
        colliderRadius_ = collider_.radius;
    }

    private void Start()
    {
        basePosition_ = transform_.position;
        playerScript_ = SceneGlobals.Instance.PlayerScript;
        playerLayer_ = SceneGlobals.Instance.PlayerLayer;
        cameraShake_ = SceneGlobals.Instance.CameraShake;
        entryPoint_ = EntryPoint.position;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer_)
        {
            Vector3 vec = collision.gameObject.transform.position - entryPoint_;
            Vector3 dir = vec.normalized;
            float relativeDistance = (colliderRadius_ - vec.magnitude) / colliderRadius_;
            float sign = Mathf.Sign(relativeDistance);
            float strength = -(relativeDistance * relativeDistance * PullForce * sign);
            cameraShake_.SetShake(strength);
            playerScript_.SetForce(dir * strength);

            if (!enterTriggered_ && relativeDistance > 0.95f)
            {
                enterTriggered_ = true;
                OnPlayerEnter.Invoke();
            }
        }
    }

    void Update()
    {
        transform_.localPosition = basePosition_ + new Vector3(0, Mathf.Sin(Time.unscaledTime * FloatSpeed) * FloatDistance, 0);
        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond);
    }
}
