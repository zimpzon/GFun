using GFun;
using UnityEngine;

public class PlayableCharacterScript : MonoBehaviour, IMovableActor, IPhysicsActor
{
    public string Name;
    public float Speed = 10;
    public int Life;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;
    public float Drag = 1.0f;
    public GameObject Blip;
    public bool ShowCollisionDebug;
    public int ShowCollisionDebugSize = 10;
    public bool IsDead = false;
    public Vector3 WeaponOffsetRight;

    public IWeapon CurrentWeapon;
    public GameObject CurrentWeaponGo;
    Transform weaponTransform_;
    SpriteRenderer weaponRenderer_;

    HumanPlayerController humanPlayerController_;
    Transform transform_;
    SpriteRenderer renderer_;
    Rigidbody2D body_;
    CameraPositioner camPositioner_;
    MapScript map_;
    bool flipX_;
    Vector3 lookAt_;
    Vector3 force_;
    Vector3 moveRequest_;
    Vector3 latestFixedMovenent_;

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

    public void AttachWeapon(GameObject weapon)
    {
        if (CurrentWeaponGo != null)
            Destroy(CurrentWeaponGo);

        CurrentWeaponGo = weapon;
        CurrentWeapon = weapon.GetComponent<IWeapon>();
        CurrentWeapon.SetForceReceiver(this);

        weaponRenderer_ = CurrentWeaponGo.GetComponentInChildren<SpriteRenderer>();
        weaponTransform_ = weapon.transform;
        weaponTransform_.localPosition = Vector3.zero;
        weapon.transform.SetParent(transform_, worldPositionStays: false);
    }

    public void SetMinimumForce(Vector3 force)
    {
        if (force.sqrMagnitude > force_.sqrMagnitude)
            force_ = force;
    }

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        Life = Mathf.Min(0, Life - amount);
        if (Life == 0)
            Die();
    }

    public void Die()
    {
        IsDead = true;
        SetIsHumanControlled(false);
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

        AttachWeapon(Weapons.Instance.CreateWeapon(WeaponIds.Rifle));
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
        moveRequest_ = isNormalized ? vector : vector.normalized;
    }

    void FixedUpdateInternal(float dt)
    {
        Vector3 movement = moveRequest_ * Speed * dt + force_ * dt;
        latestFixedMovenent_ = movement;
        moveRequest_ = Vector3.zero;

        if (force_.sqrMagnitude > 0.0f)
        {
            float forceLen = force_.magnitude;
            forceLen = Mathf.Clamp(forceLen - Drag * dt, 0.0f, float.MaxValue);
            force_ = force_.normalized * forceLen;
        }

        bool isRunning = movement != Vector3.zero;
        if (isRunning)
            body_.MovePosition(transform_.position + movement);
    }

    void UpdateInternal(float dt)
    {
        bool hasRecentlyFiredWeapon = CurrentWeapon.LatestFiringTime > Time.time - 1.5f;
        Vector3 facingDirection = hasRecentlyFiredWeapon ? CurrentWeapon.LatestFiringDirection : latestFixedMovenent_;

        bool isRunning = latestFixedMovenent_ != Vector3.zero;
        if (isRunning)
        {
            flipX_ = facingDirection.x < 0;
            lookAt_ = transform_.position + latestFixedMovenent_ * LookAtOffset;
            weaponTransform_.localPosition = WeaponOffsetRight;
        }

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;

        // Weapon positioning
        var weaponOffset = WeaponOffsetRight;
        weaponOffset.x *= flipX_ ? -1 : 1;
        weaponTransform_.localPosition = weaponOffset;
        weaponRenderer_.flipX = flipX_;
        float weaponRotation = 0;
        if (facingDirection.y < 0)
            weaponRotation = -90;
        else if (facingDirection.y > 0)
            weaponRotation = 90;

        weaponTransform_.rotation = Quaternion.Euler(0, 0, weaponRotation);

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

    void Update()
    {
        UpdateInternal(Time.unscaledDeltaTime);
    }

    void FixedUpdate()
    {
        FixedUpdateInternal(Time.fixedUnscaledDeltaTime);
    }
}
