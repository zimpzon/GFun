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
    bool flipX_;
    Vector3 moveVec_;
    AudioSource audioSource_;

    void Awake()
    {
        transform_ = transform;
        renderer_ = GetComponent<SpriteRenderer>();
        body_ = GetComponent<Rigidbody2D>();
        audioSource_ = GetComponent<AudioSource>();
    }

    private void Start()
    {
        map_ = SceneGlobals.Instance.MapScript;
    }

    public void TalkToMe()
    {
        audioSource_.Play();
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
