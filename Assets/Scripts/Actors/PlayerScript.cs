using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float Speed = 10;
    public SpriteAnimationFrames_IdleRun Anim;
    public float LookAtOffset = 10;
    public GameObject BulletPrefab;
    public Transform ShootOrigin;
    public float Drag = 1.0f;
    public bool CanAttack = true;
    public bool CanMove = true;

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

    public void AddForce(Vector3 force)
    {
        force_ += force;
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

        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
        renderer_.flipX = flipX_;

        camPositioner_.Target = lookAt_;
    }

    void FixedUpdate()
    {
        SceneGlobals.Instance.DebugLinesScript.SetLine("Fixed", Time.fixedUnscaledDeltaTime);
        UpdatePlayer(Time.fixedUnscaledDeltaTime);
    }

    void ToggleBulletTime()
    {
        bulletTime_ = !bulletTime_;
        bulletTimeTarget_ = bulletTime_ ? 1.0f : 0.0f;
    }

    void UpdateBulletTime()
    {
        SceneGlobals.Instance.DebugLinesScript.SetLine("bulletTimeValue_", bulletTimeValue_);

        lightingImageEffect_.MonochromeAmount = 2.0f * bulletTimeValue_;
        Time.timeScale = 1.0f - bulletTimeValue_ * 0.995f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        float diff = bulletTimeTarget_ - bulletTimeValue_;
        bulletTimeValue_ += diff * Time.unscaledDeltaTime * 30;
        bulletTimeValue_ = Mathf.Clamp01(bulletTimeValue_);
    }

    void Fire(Vector3 direction)
    {
        var bullet = Instantiate(BulletPrefab).GetComponent<BulletScript>();
        bullet.gameObject.SetActive(true);
        bullet.Init(ShootOrigin.position + direction * 0.25f, direction, 30, 15);

        // Don't add force when already running in shooting direction
        if (direction != moveVec_)
            AddForce(direction * 3);
    }

    void CheckAttack()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            Fire(Vector3.down);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            Fire(Vector3.up);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            Fire(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            Fire(Vector3.right);

        if (Input.GetKeyDown(KeyCode.Space))
            ToggleBulletTime();
    }

    void Update()
    {
        UpdateBulletTime();
        if (CanAttack)
            CheckAttack();

        var walls = SceneGlobals.Instance.MapScript.WallTileMap;
        var cell = walls.WorldToCell(transform_.position);
        SceneGlobals.Instance.DebugLinesScript.SetLine("Player cell", cell);

        if (Input.GetKey(KeyCode.F))
            map_.ExplodeWalls(transform.position, 5f);
    }
}
