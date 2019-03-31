using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampScript : MonoBehaviour
{
    public string EnterPortalSceneName;
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
    }

    public void OnPlayerEnterStartPortal()
    {
        StartCoroutine(PlayerEnteredPortal());
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
            fade += Time.unscaledDeltaTime;

            lightingImageEffect_.MonochromeAmount = fade;

            float scale = Mathf.Max(0.0f, 1.0f - fade);
            playerScript_.gameObject.transform.localScale = new Vector3(scale, scale, 1);

            camShake_.SetShake(1);
            yield return null;
        }

        lightingImageEffect_.Brightness = 0.0f;

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
