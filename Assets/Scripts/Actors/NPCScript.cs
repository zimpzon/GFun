using GFun;
using UnityEngine;

public class NPCScript : MonoBehaviour
{
    public float Speed = 10;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;

    Transform transform_;
    SpriteRenderer renderer_;
    Rigidbody2D body_;
    CameraPositioner camPositioner_;
    MapScript map_;
    LightingImageEffect lightingImageEffect_;
    bool flipX_;
    Vector3 lookAt_;
    Vector3 moveVec_;

    void Awake()
    {
        transform_ = transform;
        renderer_ = GetComponent<SpriteRenderer>();
        body_ = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        map_ = SceneGlobals.Instance.MapScript;
        lookAt_ = transform_.position;
        camPositioner_ = SceneGlobals.Instance.CameraPositioner;
        camPositioner_.Target = lookAt_;
        camPositioner_.SetPosition(lookAt_);
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }

    void UpdatePlayer(float dt)
    {
        moveVec_ = Vector3.zero;

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;

        camPositioner_.Target = lookAt_;
    }

    void FixedUpdate()
    {
        UpdatePlayer(Time.fixedUnscaledDeltaTime);
    }

    void Update()
    {
    }
}
