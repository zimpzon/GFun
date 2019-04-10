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
    CameraPositioner camPos_;
    CameraShake camShake_;
    LightingImageEffect lightingImageEffect_;
    IMapAccess mapAccess_;
    MapScript mapScript_;

    void Start()
    {
        camPos_ = SceneGlobals.Instance.CameraPositioner;
        camShake_ = SceneGlobals.Instance.CameraShake;
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        mapAccess_ = SceneGlobals.Instance.MapAccess;
        mapScript_ = SceneGlobals.Instance.MapScript;

        mapAccess_.BuildCollisionMapFromFloorTilemap(mapScript_.FloorTileMap);
        SetLighting(MenuLightingSettings);

        SceneGlobals.Instance.PlayableCharacters.SwitchToCharacter(PlayableCharacters.DefaultCharacter, showChangeEffect: false);

        StartCoroutine(InMenu());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SceneGlobals.Instance.PlayableCharacters.SwitchToCharacter(PlayableCharacters.PhilBeans, showChangeEffect: true);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SceneGlobals.Instance.PlayableCharacters.SwitchToCharacter(PlayableCharacters.PhilBeans2, showChangeEffect: true);
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
        LightingFadeTo(EnterPortalLightingSettings, transitionSpeed: 2);

        float fade = 0.0f;
        while (fade < 1.0f)
        {
            fade += Time.unscaledDeltaTime * 0.75f;

            float scale = Mathf.Max(0.2f, 1.0f - fade);

            camShake_.SetMinimumShake(1);
            yield return null;
        }

        LoadingCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }
}
