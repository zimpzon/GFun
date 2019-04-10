using UnityEngine;

public class HumanPlayerController : MonoBehaviour
{
    public LightingEffectSettings BulletTimeLight;

    LightingEffectSettings bulletTimeLight_ = new LightingEffectSettings();
    IMovableActor movable_;
    MapScript map_;
    bool bulletTime_;
    float bulletTimeValue_;
    float bulletTimeTarget_;
    LightingImageEffect lightingImageEffect_;
    
    private void Start()
    {
        BulletTimeLight.AmbientLight = new Color(0.9f, 0.9f, 0.9f);
        BulletTimeLight.Brightness = 1.8f;
        BulletTimeLight.MonochromeDisplayR = 0.6f;
        BulletTimeLight.MonochromeDisplayB = 0.8f;
        BulletTimeLight.MonochromeAmount = 1.5f;
        BulletTimeLight.CopyTo(bulletTimeLight_);

        movable_ = GetComponent<PlayableCharacterScript>();
        map_ = SceneGlobals.Instance.MapScript;
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        SceneGlobals.NullCheck(movable_);
    }

    // Value changed in editor
    private void OnValidate()
    {
        BulletTimeLight.CopyTo(bulletTimeLight_);
    }

    // Abilities? Shoot (arrows), time (space), active ability (q), reload (r), interact (e), bomb (f)
    void ToggleBulletTime()
    {
        bulletTime_ = !bulletTime_;
        bulletTimeTarget_ = bulletTime_ ? 1.0f : 0.0f;

        bulletTimeLight_.MonochromeAmount = bulletTime_ ? BulletTimeLight.MonochromeAmount : 0.0f;
        lightingImageEffect_.SetBaseColorTarget(bulletTimeLight_);
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

    void Update()
    {
        CheckInput();
        UpdateBulletTime();
    }

    void Fire(Vector3 direction)
    {

    }

    void ReleaseFire()
    {

    }

    void CheckInput()
    {
        var horz = Input.GetAxisRaw("Horizontal");
        var vert = Input.GetAxisRaw("Vertical");
        var moveVec = new Vector3(horz, vert);
        movable_.SetMovementVector(moveVec);

        if (Input.GetKeyDown(KeyCode.T))
            ToggleBulletTime();

        if (Input.GetKeyDown(KeyCode.F))
            map_.ExplodeWalls(transform.position, 5f);

        if (Input.GetKeyDown(KeyCode.E))
            PlainBulletGun.EffectsOn = !PlainBulletGun.EffectsOn;

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

        //if (Input.GetKeyDown(KeyCode.R))
        //    RandomizeWeapon();
    }
}
