﻿using GFun;
using UnityEngine;

public class PlayableCharacterScript : MonoBehaviour, IPhysicsActor, IEnergyProvider
{
    public string Name;
    public float Speed = 10;
    public int MaxLife = 5;
    public int Life = 5;
    public int MaxEnergy = 1000;
    public float Energy = 1000;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;
    public float Drag = 1.0f;
    public GameObject Blip;
    public bool ShowCollisionDebug;
    public int ShowCollisionDebugSize = 10;
    public bool IsDead = false;
    public Vector3 WeaponOffsetRight;
    public AudioClip TakeDamageSound;

    public IWeapon CurrentWeapon;
    public GameObject CurrentWeaponGo;
    Transform weaponTransform_;
    SpriteRenderer weaponRenderer_;
    Material renderMaterial_;

    float timeLatestEnergyUsage_;
    bool energyDepleted_;
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
        CurrentWeapon.SetOwner(this, this);

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

    public void AddForce(Vector3 force)
        => body_.AddForce(force, ForceMode2D.Impulse);

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

    public bool TryUseEnergy(float amount)
    {
        if (energyDepleted_)
            return false;

        if (Energy < amount)
        {
            energyDepleted_ = true;
            Energy = 0;
            return false;
        }

        timeLatestEnergyUsage_ = Time.unscaledTime;
        Energy -= amount;
        return true;
    }

    void UpdateEnergy()
    {
        if (EnergyWidget.Instance != null)
            EnergyWidget.Instance.ShowEnergy((int)Energy, MaxEnergy);

        if (Time.unscaledTime > timeLatestEnergyUsage_ + 0.2f)
        {
            float energyPct = Mathf.Min(Energy / MaxEnergy + 0.2f);
            if (energyPct > 0.2f)
                energyDepleted_ = false;

            float gain = Time.unscaledDeltaTime * 500 * energyPct;
            Energy = Mathf.Min(MaxEnergy, Energy + gain);
        }
    }

    public void AddHealth(int amount)
    {
        if (IsDead)
            return;

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

        Life = Mathf.Max(0, Life - amount);
        GameEvents.RaisePlayerDamaged(enemy);

        UpdateHealth();
        if (Life == 0)
        {
            Die();
            GameEvents.RaisePlayerKilled(enemy);
        }
    }

    public void Die()
    {
        DoFlash(0, 0);
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
        renderMaterial_ = renderer_.material;
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

        UpdateEnergy();
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
            body_.MovePosition(transform_.position + movement);
    }

    void UpdateInternal(float dt)
    {
        UpdateEnergy();
        UpdateFlash();

        bool hasRecentlyFiredWeapon = CurrentWeapon.LatestFiringTimeUnscaled > Time.unscaledTime - 0.70f;
        Vector3 latestHorizontalMovement = new Vector3(latestFixedMovenentDirection_.x, 0, 0);
        Vector3 facingDirection = hasRecentlyFiredWeapon ? CurrentWeapon.LatestFiringDirection : latestHorizontalMovement;
        flipX_ = facingDirection.x < 0; // NB: This makes the gun always face right when player is idle (x is 0)
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
