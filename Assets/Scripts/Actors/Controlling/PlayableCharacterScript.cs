using UnityEngine;

public class PlayableCharacterScript : MonoBehaviour, IMovableActor, IPhysicsActor
{
    public string Name;
    public float Speed = 10;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;
    public float Drag = 1.0f;
    public GameObject Blip;
    public bool ShowCollisionDebug;
    public int ShowCollisionDebugSize = 10;

    HumanPlayerController humanPlayerController_;
    Transform transform_;
    SpriteRenderer renderer_;
    Rigidbody2D body_;
    CameraPositioner camPositioner_;
    MapScript map_;
    bool flipX_;
    Vector3 lookAt_;
    Vector3 force_;
    Vector3 moveVec_;
    bool isHumanControlled_;
    InteractableTrigger switchPlayerInteract_;

    public void SetIsHumanControlled(bool isHumanControlled, bool showChangeEffect = false)
    {
        bool noChange = isHumanControlled == isHumanControlled_;
        if (noChange)
            return;

        isHumanControlled_ = isHumanControlled;
        humanPlayerController_.enabled = isHumanControlled_;
        if (showChangeEffect && isHumanControlled)
        {
            ParticleScript.EmitAtPosition(SceneGlobals.Instance.ParticleScript.CharacterSelectedParticles, transform_.position + Vector3.up * 0.5f, 30);
        }

        RefreshInteracting();
    }

    public void SetMinimumForce(Vector3 force)
    {
        if (force.sqrMagnitude > force_.sqrMagnitude)
            force_ = force;
    }

    public void SetForce(Vector3 force)
    {
        force_ = force;
    }

    public void OnPlayerSwitchSelected()
    {
        PlayableCharacters.Instance.SetCharacterToHumanControlled(tag, showChangeEffect: true);
    }

    public void RefreshInteracting()
    {
        switchPlayerInteract_.Message = $"Take Over {Name}";
        switchPlayerInteract_.OnAccept.AddListener(OnPlayerSwitchSelected);
        switchPlayerInteract_.gameObject.SetActive(!isHumanControlled_);
    }

    void Awake()
    {
        transform_ = transform;
        humanPlayerController_ = GetComponent<HumanPlayerController>();
        renderer_ = GetComponent<SpriteRenderer>();
        body_ = GetComponent<Rigidbody2D>();
        switchPlayerInteract_ = transform_.Find("SwitchPlayerInteract").GetComponent<InteractableTrigger>();
    }

    private void Start()
    {
        Blip.SetActive(true);
        map_ = SceneGlobals.Instance.MapScript;
        lookAt_ = transform_.position;
        camPositioner_ = SceneGlobals.Instance.CameraPositioner;

        RefreshInteracting();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        force_ = Vector3.zero;
    }

    public void SetMovementVector(Vector3 vector, bool isNormalized = true)
    {
        moveVec_ = isNormalized ? vector : vector.normalized;
    }

    void UpdateInternal(float dt)
    {
        Vector3 movement = moveVec_ * Speed * dt + force_ * dt;
        moveVec_ = Vector3.zero;

        if (force_.sqrMagnitude > 0.0f)
        {
            float forceLen = force_.magnitude;
            forceLen = Mathf.Clamp(forceLen - Drag * dt, 0.0f, float.MaxValue);
            force_ = force_.normalized * forceLen;
        }

        bool isRunning = movement != Vector3.zero;
        if (isRunning)
        {
            body_.MovePosition(transform_.position + movement);
            flipX_ = movement.x < 0;
            lookAt_ = transform_.position + movement * LookAtOffset;
        }

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;

        if (isHumanControlled_)
            camPositioner_.Target = lookAt_;

        if (ShowCollisionDebug)
            DrawCollisionDebug();
    }

    void DrawCollisionDebug()
    {
        var collPos = map_.GetCollisionTilePosFromWorldPos(transform_.position);
        SceneGlobals.Instance.DebugLinesScript.SetLine("Collision cell", collPos);
        var worldPos = map_.GetWorldPosFromCollisionTileCenter(collPos.x, collPos.y);
        SceneGlobals.Instance.DebugLinesScript.SetLine("Collision worldPos", worldPos);

        int Radius = ShowCollisionDebugSize;
        for (int y = collPos.y - Radius; y <= collPos.y + Radius; ++y)
        {
            for (int x = collPos.x - Radius; x <= collPos.x + Radius; ++x)
            {
                map_.DebugDrawCollisionTile(x, y, dark: x == collPos.x && y == collPos.y);
            }
        }
    }

    void FixedUpdate()
    {
        UpdateInternal(Time.fixedUnscaledDeltaTime);
    }
}
