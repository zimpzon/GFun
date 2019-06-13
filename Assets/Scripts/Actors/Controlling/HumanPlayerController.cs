using GFun;
using UnityEngine;

public class HumanPlayerController : MonoBehaviour
{
    public LightingEffectSettings BulletTimeLight;

    public static TrackedPath TrackedPath = new TrackedPath();
    public static bool Disabled = false;
    public static bool CanShoot = true;

    PlayableCharacterScript player_;
    Transform transform_;
    MapScript map_;
    bool bulletTime_;
    float bulletTimeValue_;
    float bulletTimeTarget_;
    bool isMoving_;
    Camera mainCam_;

    private void Start()
    {
        mainCam_ = Camera.main;

        transform_ = transform;
        player_ = GetComponent<PlayableCharacterScript>();
        SceneGlobals.NullCheck(player_);
        map_ = SceneGlobals.Instance.MapScript;

        TrackedPath.Rewind();
    }

    void ToggleBulletTime()
    {
        bulletTime_ = !bulletTime_;
        AiBlackboard.Instance.BulletTimeActive = bulletTime_;
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
    }

    private void OnDisable()
    {
        player_.CurrentWeapon.OnTriggerUp();
    }

    private void FixedUpdate()
    {
        if (Disabled)
            return;

        CheckMovement();
    }

    void Update()
    {
        if (Disabled)
            return;

        CheckActions();
        UpdateBulletTime();

        AiBlackboard.Instance.PlayerPosition = transform_.position;
    }

    void Fire(Vector3 direction)
    {
        if (!CanShoot)
            return;

        player_.CurrentWeapon.OnTriggerDown(direction);
    }

    void ReleaseFire()
    {
        player_.CurrentWeapon.OnTriggerUp();
    }

    int screenshotCounter = 0;
    void CheckActions()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Application.isEditor)
            ScreenCapture.CaptureScreenshot($@"C:\private\GFun\Screenshots\screenshot{screenshotCounter++}.png");

        //if (Input.GetKeyDown(KeyCode.Q))
        //    ToggleBulletTime();

        if (Input.GetKeyDown(KeyCode.F))
            Screen.fullScreen = !Screen.fullScreen;

        //if (Input.GetKeyDown(KeyCode.F))
        //    MapScript.Instance.TriggerExplosion(transform_.position, 5);

        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCam_.transform.position.z;
        var mouseWorldPos = mainCam_.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        var weaponMuzzlePosition = player_.CurrentWeapon.GetMuzzlePosition(mouseWorldPos);
        var lookDir = (mouseWorldPos - weaponMuzzlePosition).normalized;

        if (Input.GetMouseButton(0))
            Fire(lookDir);
        if (Input.GetMouseButtonUp(0))
            ReleaseFire();

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

    void CheckMovement()
    {
        var horz = Input.GetAxisRaw("Horizontal");
        var vert = Input.GetAxisRaw("Vertical");
        var moveVec = new Vector3(horz, vert).normalized;
        player_.Move(moveVec);
        isMoving_ = moveVec != Vector3.zero;

        TrackedPath.Sample(moveVec, transform_.position, Time.unscaledTime);
    }
}
