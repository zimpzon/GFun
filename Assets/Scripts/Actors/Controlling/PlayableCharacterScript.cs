using GFun;
using UnityEngine;

public class PlayerSelfDamage : IEnemy
{
    public static readonly PlayerSelfDamage Instance = new PlayerSelfDamage();

    public EnemyId Id => EnemyId.PlayerSelfDamage;
    public string Name => "Yourself";
    public int Level => 1;
    public float Life => 1;
    public float LifePct => 1;
    public float MaxLife => 1;
    public int XP => 0;
    public bool LootDisabled => true;
    public bool IsDead => false;
    public void DoFlash(float amount, float ms) { }
    public void TakeDamage(int amount, Vector3 damageForce) { }
}

public class PlayerHealthEvent
{
    public int MaxHealth;
    public int HealthBefore;
    public int HealthChange;
    public string ChangeSource;
    public float Time;
}

public class PlayableCharacterScript : MonoBehaviour, IPhysicsActor
{
    public string Name;
    public float Speed = 10;
    public int MaxLife = 5;
    public int Life = 5;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;
    public float Drag = 1.0f;
    public GameObject Blip;
    public bool ShowCollisionDebug;
    public int ShowCollisionDebugSize = 10;
    public bool IsDead = false;
    public Vector3 WeaponOffsetRight;
    public AudioClip TakeDamageSound;
    public Texture2D CursorTexture;

    [System.NonSerialized] public string KilledBy;
    [System.NonSerialized] public IEnemy KilledByEnemy;

    public IWeapon CurrentWeapon;
    public GameObject CurrentWeaponGo;
    Transform weaponTransform_;
    SpriteRenderer weaponRenderer_;
    Material renderMaterial_;
    Camera mainCam_;

    float flashAmount_;
    float flashEndTime_;
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
    Vector3 latestFixedMovenentDirection_;
    float latestFacingDirection_ = 1;
    Collider2D collider_;

    bool isHumanControlled_;
    InteractableTrigger switchPlayerInteract_;

    void AddPlayerHealthEvent(int amount, string source)
    {
        CurrentRunData.Instance.AddPlayerHealthEvent(amount, source);
    }

    public void DisableCollider()
    {
        collider_.enabled = false;
    }

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
        CurrentWeapon.SetOwner(this);

        weaponRenderer_ = CurrentWeaponGo.GetComponentInChildren<SpriteRenderer>();
        weaponTransform_ = weapon.transform;
        weaponTransform_.localPosition = Vector3.zero;
        weapon.transform.SetParent(transform_, worldPositionStays: false);

        humanPlayerController_.UpdateWeapon();
    }

    public void SetMinimumForce(Vector3 force)
    {
        if (force.sqrMagnitude > force_.sqrMagnitude)
            force_ = force;
    }

    public void AddForce(Vector3 force, ForceMode2D forceMode = ForceMode2D.Impulse)
        => body_.AddForce(force, forceMode);

    void DoFlash(float amount, float ms)
    {
        flashEndTime_ = Time.unscaledTime + ms;
        flashAmount_ = amount;
        renderMaterial_.SetFloat("_FlashAmount", flashAmount_);
    }

    void UpdateFlash()
    {
        if (flashAmount_ > 0.0f && Time.unscaledTime > flashEndTime_)
        {
            flashAmount_ = 0.0f;
            renderMaterial_.SetFloat("_FlashAmount", 0.0f);
        }
    }

    void UpdateHealth()
    {
        if (HealthWidget.Instance != null)
            HealthWidget.Instance.ShowLife(Life, MaxLife);
    }

    public void AddHealth(int amount, string source)
    {
        if (IsDead)
            return;

        FloatingTextSpawner.Instance.Spawn(transform_.position + Vector3.up, $"+{amount.ToString()}", Color.green, speed: 1.0f, timeToLive: 2.0f);
        AddPlayerHealthEvent(amount, source);
        Life = Mathf.Min(MaxLife, Life + amount);
        UpdateHealth();
    }

    public void AddMaxHealth(int amount)
    {
        if (IsDead)
            return;

        MaxLife += amount;
        UpdateHealth();
    }

    public void TakeDamage(IEnemy enemy, int amount, Vector3 damageForce)
    {
        if (IsDead)
            return;

        AudioManager.Instance.PlaySfxClip(TakeDamageSound, 1);
        DoFlash(2, 0.3f);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.DeathFlashParticles, transform_.position, 2);
        FloatingTextSpawner.Instance.Spawn(transform_.position + Vector3.up, $"-{amount.ToString()}", Color.red, speed: 1.0f, timeToLive: 1.0f);

        AddPlayerHealthEvent(-amount, enemy.Name);

        Life = Mathf.Max(0, Life - amount);
        GameEvents.RaisePlayerDamaged(enemy);

        UpdateHealth();
        if (Life == 0)
        {
            KilledBy = enemy.Name;
            KilledByEnemy = enemy;
            Die();
            GameEvents.RaisePlayerKilled(enemy);
        }
    }

    public void Die()
    {
        DoFlash(0, 0);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.DeathFlashParticles, transform_.position, 4);
        IsDead = true;
        SetIsHumanControlled(false);
        CameraShake.Instance.SetMinimumShake(0.8f);
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
        collider_ = GetComponent<Collider2D>();
        humanPlayerController_ = GetComponent<HumanPlayerController>();
        renderer_ = GetComponent<SpriteRenderer>();
        renderMaterial_ = renderer_.material;
        body_ = GetComponent<Rigidbody2D>();
        switchPlayerInteract_ = transform_.Find("SwitchPlayerInteract").GetComponent<InteractableTrigger>();

        AttachWeapon(Weapons.Instance.CreateWeapon(WeaponIds.Shotgun));
    }

    private void Start()
    {
        slideFilter = new ContactFilter2D() { layerMask = 1 << SceneGlobals.Instance.MapLayer, useLayerMask = true };

        Blip.SetActive(true);
        mainCam_ = Camera.main;
        map_ = SceneGlobals.Instance.MapScript;
        lookAt_ = transform_.position;
        camPositioner_ = SceneGlobals.Instance.CameraPositioner;

        UpdateHealth();

        RefreshInteracting();
    }

    public Vector3 GetPosition()
        => transform_.position;

    public void Move(Vector3 step)
    {
        moveRequest_ = step;
    }

    void FixedUpdateInternal(float dt)
    {
        Vector3 movement = moveRequest_ * Speed * dt + force_ * dt;

        latestFixedMovenentDirection_ = movement.normalized;
        moveRequest_ = Vector3.zero;

        if (force_.sqrMagnitude > 0.0f)
        {
            float forceLen = force_.magnitude;
            forceLen = Mathf.Clamp(forceLen - Drag * dt, 0.0f, float.MaxValue);
            force_ = force_.normalized * forceLen;
        }

        bool isRunning = movement != Vector3.zero;
        if (isRunning)
        {
            movement = SlideWalls(transform_.position, movement);
            body_.MovePosition(transform_.position + movement);
        }
    }

    ContactFilter2D slideFilter;
    RaycastHit2D[] slideHit = new RaycastHit2D[1];

    Vector3 SlideWalls(Vector3 pos, Vector3 movement)
    {
        // Only slide when moving diagonally
        if (movement.x == 0 || movement.y == 0)
            return movement;

        float magnitude = movement.magnitude;
        int collCount = collider_.Cast(movement.normalized, slideFilter, slideHit, magnitude);
        // If no collision in this attempted move there is nothing to adjust
        if (collCount == 0)
            return movement;

        // There is possibly something to adjust, try moving in X only.
        var moveX = new Vector3(Mathf.Sign(movement.x), 0, 0);
        collCount = collider_.Cast(moveX, slideFilter, slideHit, movement.magnitude);

        if (collCount == 0)
        {
            // There is room when moving in the X direction. Try the full movement in X.
            var result = new Vector3(Mathf.Sign(movement.x) * magnitude, 0, 0);
            return result;
        }

        // There is possibly something to adjust, try moving in Y only.
        var moveY = new Vector3(0, Mathf.Sign(movement.y), 0);
        collCount = collider_.Cast(moveY, slideFilter, slideHit, movement.magnitude);

        if (collCount == 0)
        {
            // There is room when moving in the Y direction. Try the full movement in Y.
            var result = new Vector3(0, Mathf.Sign(movement.y) * magnitude, 0);
            return result;
        }

        // There is collision in both X and Y, don't attempt to adjust anything.
        return movement;
    }

    void UpdateInternal(float dt)
    {
        UpdateFlash();

        UpdateInternal_MouseControls(dt);

        if (isHumanControlled_)
            camPositioner_.Target = lookAt_;

        if (ShowCollisionDebug)
            DrawCollisionDebug();
    }

    void UpdateInternal_KeyboardControls(float dt)
    {
        bool hasRecentlyFiredWeapon = CurrentWeapon.LatestFiringTimeUnscaled > Time.unscaledTime - 0.70f;
        Vector3 latestHorizontalMovement = new Vector3(latestFixedMovenentDirection_.x, 0, 0);
        Vector3 facingDirection = hasRecentlyFiredWeapon ? CurrentWeapon.LatestFiringDirection : latestHorizontalMovement;

        if (facingDirection.x != 0)
            latestFacingDirection_ = facingDirection.x;

        flipX_ = latestFacingDirection_ < 0;
        weaponTransform_.localPosition = WeaponOffsetRight;

        bool isRunning = latestFixedMovenentDirection_ != Vector3.zero;
        if (isRunning)
            lookAt_ = transform_.position + latestFixedMovenentDirection_ * LookAtOffset;

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
    }

    void UpdateInternal_MouseControls(float dt)
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCam_.transform.position.z;
        var mouseWorldPos = mainCam_.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        var weaponMuzzlePosition = CurrentWeapon.GetMuzzlePosition(mouseWorldPos);
        var lookDir = (mouseWorldPos - weaponMuzzlePosition).normalized;
        float distanceToCursor = (mouseWorldPos - weaponMuzzlePosition).magnitude;

        flipX_ = lookDir.x < 0;
        weaponTransform_.localPosition = WeaponOffsetRight;

        bool isRunning = latestFixedMovenentDirection_ != Vector3.zero;
        lookAt_ = weaponMuzzlePosition + lookDir * distanceToCursor * 0.15f;

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;

        // Weapon positioning
        var weaponOffset = WeaponOffsetRight;
        weaponOffset.x *= flipX_ ? -1 : 1;
        weaponTransform_.localPosition = weaponOffset;
        weaponRenderer_.flipX = flipX_;

        float weaponRotation = Mathf.Atan2(lookDir.x, -lookDir.y) * Mathf.Rad2Deg - (flipX_ ? 270 : 90);
        weaponTransform_.rotation = Quaternion.Euler(0, 0, weaponRotation);
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
