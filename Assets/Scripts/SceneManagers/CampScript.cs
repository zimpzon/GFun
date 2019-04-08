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

    PlayerScript playerScript_;
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

        StartCoroutine(InMenu());
    }

    public void OnPlayerEnterStartPortal()
    {
        StopAllCoroutines();
        StartCoroutine(PlayerEnteredPortal());
    }

    IEnumerator InCamp()
    {
        var lightingSettings = new LightingEffectSettings();
        lightingSettings.SetDefaults();
        lightingImageEffect_.SetBaseColorTarget(lightingSettings);

        IntroCanvas.enabled = false;
        playerScript_.CanMove = true;
        SceneGlobals.Instance.AudioManager.StopMusic();

        while (true)
        {
            // Just an example of easy debug info. Can be deleted.
            SceneGlobals.Instance.DebugLinesScript.SetLine("Example debug info", "In camp, deltatime: " + Time.unscaledDeltaTime);

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
        playerScript_.CanMove = false;
        IntroCanvas.enabled = true;

        var lightingSettings = new LightingEffectSettings();
        lightingSettings.SetDefaults();
        lightingSettings.MonochromeAmount = 1.0f;
        lightingSettings.MonochromeDisplayR = 1.0f;
        lightingSettings.MonochromeDisplayG = 1.0f;
        lightingSettings.MonochromeDisplayB = 1.0f;
        lightingSettings.Brightness = 0.75f;
        lightingImageEffect_.SetBaseColorTarget(lightingSettings, 30);

        SceneGlobals.Instance.AudioManager.PlayMusic(IntroMusicClip);

        while (true)
        {
            // Just an example of easy debug info. Can be deleted.
            SceneGlobals.Instance.DebugLinesScript.SetLine("Example debug info", "In menu, deltatime: " + Time.unscaledDeltaTime);

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

        var lightingSettings = new LightingEffectSettings();
        lightingSettings.SetDefaults();
        lightingSettings.MonochromeAmount = 1.0f;
        lightingSettings.MonochromeDisplayR = 0.33f;
        lightingSettings.MonochromeDisplayG = 0.33f;
        lightingSettings.MonochromeDisplayB = 0.33f;
        lightingImageEffect_.SetBaseColorTarget(lightingSettings, 10);

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

        lightingSettings.Brightness = 0.0f;
        lightingImageEffect_.SetBaseColor(lightingSettings);

        LoadingCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }
}
