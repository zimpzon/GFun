using GFun;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float Speed = 10;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;
    public Transform ShootOrigin;
    public float Drag = 1.0f;
    public bool CanAttack = true;
    public bool CanMove = true;
    public GameObject DefaultWeapon;
    public GameObject ActiveWeapon;
    public Transform WeaponPositionRight;
    public GameObject Blip;

    Transform transform_;
    SpriteRenderer renderer_;
    Rigidbody2D body_;
    CameraPositioner camPositioner_;
    MapScript map_;
    LightingImageEffect lightingImageEffect_;
    bool flipX_;
    Vector3 lookAt_;
    Vector3 force_;
    bool bulletTime_;
    float bulletTimeValue_;
    float bulletTimeTarget_;
    Vector3 moveVec_;
    IWeapon activeWeaponScript_;

    public void SetMinimumForce(Vector3 force)
    {
        if (force.sqrMagnitude > force_.sqrMagnitude)
            force_ = force;
    }

    public void SetForce(Vector3 force)
    {
        force_ = force;
    }

    void Awake()
    {
        transform_ = transform;
        renderer_ = GetComponent<SpriteRenderer>();
        body_ = GetComponent<Rigidbody2D>();
        Blip.SetActive(true);
    }

    private void Start()
    {
        map_ = SceneGlobals.Instance.MapScript;
        lookAt_ = transform_.position;
        camPositioner_ = SceneGlobals.Instance.CameraPositioner;
        camPositioner_.Target = lookAt_;
        camPositioner_.SetPosition(lookAt_);
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;

        SetActiveWeaponFromPrefab(DefaultWeapon);
    }

    void SetActiveWeaponFromPrefab(GameObject prefab)
    {
        var instantiated = Instantiate(prefab);
        SetActiveWeaponFromInstantiated(instantiated);
    }

    void SetActiveWeaponFromInstantiated(GameObject weapon)
    {
        ActiveWeapon = weapon;
        ActiveWeapon.transform.SetParent(transform_);
        ActiveWeapon.transform.localPosition = Vector3.zero;

        ActiveWeapon.SetActive(true);
        activeWeaponScript_ = ActiveWeapon.GetComponent<IWeapon>();
        activeWeaponScript_.SetForceCallback(WeaponForceCallback);
    }

    private void RandomizeWeapon()
    {
        var gun = ActiveWeapon.GetComponent<PlainBulletGun>();
        gun.GunSettings.BurstCount = Random.Range(1, 4);
        gun.GunSettings.FiringMode = (FiringMode)Random.Range(1, 3);
        gun.GunSettings.FiringSpread = (FiringSpread)Random.Range(0, 3);

        string burstName = "";
        if (gun.GunSettings.BurstCount == 2)
            burstName = "Dual Burst ";
        else if (gun.GunSettings.BurstCount == 3)
            burstName = $"Tripple Burst ";

        string autoName = gun.GunSettings.FiringMode == FiringMode.Auto ? "Auto " : "";

        string spreadName = "";
        if (gun.GunSettings.FiringSpread == FiringSpread.Dual)
            spreadName = "Double Shot ";
        else if (gun.GunSettings.FiringSpread == FiringSpread.Tripple)
            spreadName = "Tripple Shot ";

        string name = $"{autoName}{burstName}{spreadName}Rifle";
        SceneGlobals.Instance.DebugLinesScript.SetLine("Gun", name);
    }

    void WeaponForceCallback(Vector3 force)
    {
        SetMinimumForce(force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        force_ = Vector3.zero;
    }

    void UpdatePlayer(float dt)
    {
        moveVec_ = Vector3.zero;
        if (CanMove)
        {
            var horz = Input.GetAxisRaw("Horizontal");
            var vert = Input.GetAxisRaw("Vertical");
            moveVec_ = new Vector3(horz, vert);
        }

        Vector3 movement = moveVec_ * Speed * dt + force_ * dt;

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

        var weaponPos = WeaponPositionRight.transform.localPosition;
        if (flipX_)
            weaponPos.x *= -1;
        ActiveWeapon.transform.localPosition = weaponPos;

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;

        camPositioner_.Target = lookAt_;
    }

    void FixedUpdate()
    {
        UpdatePlayer(Time.fixedUnscaledDeltaTime);
    }

    void ToggleBulletTime()
    {
        bulletTime_ = !bulletTime_;
        bulletTimeTarget_ = bulletTime_ ? 1.0f : 0.0f;
    }

    void UpdateBulletTime()
    {
        if (!bulletTime_ && bulletTimeValue_ == 0.0f)
            return;

        Time.timeScale = 1.0f - bulletTimeValue_ * 0.995f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        float diff = bulletTimeTarget_ - bulletTimeValue_;
        bulletTimeValue_ += diff * Time.unscaledDeltaTime * 30;
        bulletTimeValue_ = Mathf.Clamp01(bulletTimeValue_);
        lightingImageEffect_.MonochromeAmount = 2.0f * bulletTimeValue_;
    }

    void Fire(Vector3 direction)
    {
        ActiveWeapon.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        activeWeaponScript_.OnTriggerDown();
    }

    void ReleaseFire()
    {
        activeWeaponScript_.OnTriggerUp();
    }

    void UpdateWeapon()
    {
        if (Input.GetKey(KeyCode.DownArrow))
            Fire(Vector3.down);
        else if (Input.GetKey(KeyCode.UpArrow))
            Fire(Vector3.up);
        else if (Input.GetKey(KeyCode.LeftArrow))
            Fire(Vector3.left);
        else if (Input.GetKey(KeyCode.RightArrow))
            Fire(Vector3.right);

        if (Input.GetKeyUp(KeyCode.DownArrow))
            ReleaseFire();
        else if (Input.GetKeyUp(KeyCode.UpArrow))
            ReleaseFire();
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
            ReleaseFire();
        else if (Input.GetKeyUp(KeyCode.RightArrow))
            ReleaseFire();
    }

    void Update()
    {
        UpdateBulletTime();
        if (CanAttack)
            UpdateWeapon();

        if (Input.GetKeyDown(KeyCode.F))
            map_.ExplodeWalls(transform.position, 5f);

        if (Input.GetKeyDown(KeyCode.R))
            RandomizeWeapon();

        if (Input.GetKeyDown(KeyCode.Space))
            ToggleBulletTime();

        if (Input.GetKeyDown(KeyCode.E))
            PlainBulletGun.EffectsOn = !PlainBulletGun.EffectsOn;
    }
}
