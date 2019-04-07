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

    void Start()
    {
        playerScript_ = SceneGlobals.Instance.PlayerScript;
        camPos_ = SceneGlobals.Instance.CameraPositioner;
        camShake_ = SceneGlobals.Instance.CameraShake;
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;

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
        lightingImageEffect_.MonochromeAmount = 0.0f;
        lightingImageEffect_.Brightness = 1.5f;
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

        lightingImageEffect_.MonochromeDisplayR = 1.0f;
        lightingImageEffect_.MonochromeDisplayG = 1.0f;
        lightingImageEffect_.MonochromeDisplayB = 1.0f;
        lightingImageEffect_.MonochromeAmount = 1.0f;
        lightingImageEffect_.Brightness = 0.75f;
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
        lightingImageEffect_.MonochromeDisplayR = 0.33f;
        lightingImageEffect_.MonochromeDisplayG = 0.33f;
        lightingImageEffect_.MonochromeDisplayB = 0.33f;

        float fade = 0.0f;
        while (fade < 1.0f)
        {
            // Just an example of easy debug info. Can be deleted.
            SceneGlobals.Instance.DebugLinesScript.SetLine("Example debug info", "In portal, deltatime: " + Time.unscaledDeltaTime);

            fade += Time.unscaledDeltaTime * 0.75f;
            lightingImageEffect_.MonochromeAmount = Mathf.Max(0.0f, fade * 2 - 1.0f);

            float scale = Mathf.Max(0.2f, 1.0f - fade);
            playerScript_.gameObject.transform.localScale = new Vector3(scale, scale, 1);

            camShake_.SetMinimumShake(1);
            yield return null;
        }

        lightingImageEffect_.Brightness = 0.0f;
        LoadingCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }
}
