using UnityEngine;
using UnityEngine.Events;

public class PortalScript : MonoBehaviour
{
    public Transform EntryPoint;
    public SpriteAnimationFrames_Single Anim;
    public float PullForce = 10;
    public UnityEvent OnPlayerEnter;

    SpriteRenderer renderer_;
    Transform transform_;
    Vector3 basePosition_;
    CircleCollider2D collider_;
    Vector3 entryPoint_;
    PlayableCharacterScript playerScript_;
    LayerMask playerLayer_;
    float colliderRadius_;
    CameraShake cameraShake_;
    bool enterTriggered_;
    AudioSource enterSound_;

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        transform_ = transform;
        enterSound_ = GetComponent<AudioSource>();
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
            cameraShake_.SetMinimumShake(strength);
            playerScript_.SetForce(dir * strength);

            // Point of no return
            if (!enterTriggered_ && relativeDistance > 0.5f)
            {
                enterSound_.Play();
            }

            if (!enterTriggered_ && relativeDistance > 0.95f)
            {
                enterTriggered_ = true;
                OnPlayerEnter.Invoke();
            }
        }
    }

    void Update()
    {
        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond);
    }
}
