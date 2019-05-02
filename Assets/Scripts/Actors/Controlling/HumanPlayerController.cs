using GFun;
using UnityEngine;

public class HumanPlayerController : MonoBehaviour
{
    public LightingEffectSettings BulletTimeLight;

    public static bool Disabled = false;

    LightingEffectSettings bulletTimeLight_ = new LightingEffectSettings();
    LightingEffectSettings defaultLight_ = new LightingEffectSettings();
    PlayableCharacterScript player_;
    Transform transform_;
    IWeapon weapon_;
    MapScript map_;
    bool bulletTime_;
    float bulletTimeValue_;
    float bulletTimeTarget_;
    LightingImageEffect lightingImageEffect_;
    bool isMoving_;
    
    private void Start()
    {
        BulletTimeLight.AmbientLight = new Color(0.9f, 0.9f, 0.9f);
        BulletTimeLight.Brightness = 1.8f;
        BulletTimeLight.MonochromeDisplayR = 0.6f;
        BulletTimeLight.MonochromeDisplayB = 0.8f;
        BulletTimeLight.MonochromeAmount = 1.5f;
        BulletTimeLight.CopyTo(bulletTimeLight_);

        transform_ = transform;
        player_ = GetComponent<PlayableCharacterScript>();
        SceneGlobals.NullCheck(player_);
        map_ = SceneGlobals.Instance.MapScript;

        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        lightingImageEffect_.StartValues.CopyTo(defaultLight_);

        UpdateWeapon();
    }

    public void UpdateWeapon()
    {
        weapon_ = GetComponentInChildren<IWeapon>();
    }

    // Value changed in editor
    private void OnValidate()
    {
        BulletTimeLight.CopyTo(bulletTimeLight_);
    }

    void ToggleBulletTime()
    {
        bulletTime_ = !bulletTime_;
        AiBlackboard.Instance.BulletTimeActive = bulletTime_;
        bulletTimeTarget_ = bulletTime_ ? 1.0f : 0.0f;

        if (bulletTime_)
        {
            lightingImageEffect_.SetBaseColorTarget(bulletTimeLight_);
        }
        else
        {
            lightingImageEffect_.SetBaseColorTarget(lightingImageEffect_.StartValues);
        }
    }

    void UpdateBulletTime()
    {
        if (!bulletTime_ && bulletTimeValue_ == 0.0f)
            return;

        if (bulletTime_)
        {
            float cost = isMoving_ ? 800 : 100;
            if (!player_.TryUseEnergy(cost * Time.unscaledDeltaTime))
            {
                ToggleBulletTime();
            }
        }

        Time.timeScale = 1.0f - bulletTimeValue_ * 0.995f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        float diff = bulletTimeTarget_ - bulletTimeValue_;
        bulletTimeValue_ += diff * Time.unscaledDeltaTime * 30;
        bulletTimeValue_ = Mathf.Clamp01(bulletTimeValue_);
    }

    private void OnDisable()
    {
        weapon_.OnTriggerUp();    
    }

    void Update()
    {
        if (Disabled)
            return;

        // Lighting fade test
        //float f = Mathf.Sin(Time.time) * 0.5f + 0.5f;
        //defaultLight_.AmbientLight = new Color(f, f, f);
        //lightingImageEffect_.SetBaseColorTarget(defaultLight_);

        CheckInput();
        UpdateBulletTime();

        AiBlackboard.Instance.PlayerPosition = transform_.position;
    }

    void Fire(Vector3 direction)
    {
        weapon_.OnTriggerDown(direction);
    }

    void ReleaseFire()
    {
        weapon_.OnTriggerUp();
    }

    void CheckInput()
    {
        if (MiniMapCamera.Instance.IsShown)
            return;

        var horz = Input.GetAxisRaw("Horizontal");
        var vert = Input.GetAxisRaw("Vertical");
        var moveVec = new Vector3(horz, vert);
        player_.Move(moveVec);
        isMoving_ = moveVec != Vector3.zero;

        if (Input.GetKeyDown(KeyCode.Q))
            ToggleBulletTime();

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    map_.TriggerExplosion(transform.position, 3f);
        //}

        if (Input.GetKeyDown(KeyCode.P))
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
