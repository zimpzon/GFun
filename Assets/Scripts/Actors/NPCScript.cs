using UnityEngine;
using VikingCrewTools.UI;

public class NPCScript : MonoBehaviour
{
    public float Speed = 10;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;

    Transform transform_;
    SpriteRenderer renderer_;
    Rigidbody2D body_;
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
    }

    public void TalkToMe()
    {
        SpeechBubbleManager.Instance.AddSpeechBubble(transform, "Come to relive your past lives?");
    }

    void UpdatePlayer(float dt)
    {
        moveVec_ = Vector3.zero;

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;
    }

    void FixedUpdate()
    {
        UpdatePlayer(Time.fixedUnscaledDeltaTime);
    }

    void Update()
    {
    }
}
