using GFun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampScript : MonoBehaviour
{
    public string EnterPortalSceneName;
    public Canvas IntroCanvas;
    public Canvas LoadingCanvas;
    public AudioClip IntroMusicClip;
    public LightingEffectSettings CampLightingSettings;
    public LightingEffectSettings EnterPortalLightingSettings;
    public LightingEffectSettings GraveyardLightingSettings;
    public LightingEffectSettings MenuLightingSettings;
    public AudioSource CampfireSoundSource;

    LightingEffectSettings activeLightingSettings_;
    bool isInGraveyard_;
    PlayableCharacterScript playerScript_;
    CameraPositioner camPos_;
    CameraShake camShake_;
    LightingImageEffect lightingImageEffect_;
    IMapAccess mapAccess_;
    MapScript mapScript_;

    void Start()
    {
        playerScript_ = SceneGlobals.Instance.PlayerScript;
        camPos_ = SceneGlobals.Instance.CameraPositioner;
        camShake_ = SceneGlobals.Instance.CameraShake;
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        mapAccess_ = SceneGlobals.Instance.MapAccess;
        mapScript_ = SceneGlobals.Instance.MapScript;

        mapAccess_.BuildCollisionMapFromFloorTilemap(mapScript_.FloorTileMap);

        camPos_.SetTarget(playerScript_.transform.position);
        camPos_.SetPosition(playerScript_.transform.position);

        SetLighting(MenuLightingSettings);

        StartCoroutine(InMenu());
    }

    private void Update()
    {
        SceneGlobals.Instance.DebugLinesScript.SetLine("IsInGraveyard", isInGraveyard_);
    }

    public void OnPlayerEnterStartPortal()
    {
        StopAllCoroutines();
        StartCoroutine(PlayerEnteredPortal());
    }

    public void OnGraveyardEnter()
    {
        LightingFadeTo(GraveyardLightingSettings, transitionSpeed: 5);
        isInGraveyard_ = true;
    }

    public void OnGraveyardLeave()
    {
        LightingFadeTo(CampLightingSettings, transitionSpeed: 5);
        isInGraveyard_ = false;
    }

    void LightingFadeTo(LightingEffectSettings settings, float transitionSpeed = 30)
    {
        activeLightingSettings_ = settings;
        lightingImageEffect_.SetBaseColorTarget(activeLightingSettings_, transitionSpeed: transitionSpeed);
    }

    void SetLighting(LightingEffectSettings settings)
    {
        activeLightingSettings_ = settings;
        lightingImageEffect_.SetBaseColor(activeLightingSettings_);
    }

    IEnumerator InCamp()
    {
        Time.timeScale = 1.0f;


        CampfireSoundSource.enabled = true;
        LightingFadeTo(isInGraveyard_ ? GraveyardLightingSettings : CampLightingSettings, transitionSpeed: 20);
        IntroCanvas.enabled = false;
        playerScript_.CanMove = true;
        SceneGlobals.Instance.AudioManager.StopMusic();

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(InMenu());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator InMenu()
    {
        CampfireSoundSource.enabled = false;
        Time.timeScale = 0.25f;

        playerScript_.CanMove = false;
        IntroCanvas.enabled = true;

        LightingFadeTo(MenuLightingSettings, transitionSpeed: 20);
        SceneGlobals.Instance.AudioManager.PlayMusic(IntroMusicClip);

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(InCamp());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator PlayerEnteredPortal()
    {
        playerScript_.CanMove = false;

        LightingFadeTo(EnterPortalLightingSettings, transitionSpeed: 2);

        float fade = 0.0f;
        while (fade < 1.0f)
        {
            // Just an example of easy debug info. Can be deleted.
            SceneGlobals.Instance.DebugLinesScript.SetLine("Example debug info", "In portal, deltatime: " + Time.unscaledDeltaTime);

            fade += Time.unscaledDeltaTime * 0.75f;

            float scale = Mathf.Max(0.2f, 1.0f - fade);
            playerScript_.gameObject.transform.localScale = new Vector3(scale, scale, 1);

            camShake_.SetMinimumShake(1);
            yield return null;
        }

        LoadingCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }
}
