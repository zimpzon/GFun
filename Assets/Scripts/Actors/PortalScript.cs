using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PortalScript : MonoBehaviour
{
    public Transform EntryPoint;
    public SpriteAnimationFrames_Single Anim;
    public float PullForce = 10;
    public UnityEvent OnPlayerEnter;

    Collider2D collider_;
    SpriteRenderer renderer_;
    CameraShake cameraShake_;
    AudioSource enterSound_;

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        enterSound_ = GetComponent<AudioSource>();
        cameraShake_ = SceneGlobals.Instance.CameraShake;
        collider_ = GetComponent<Collider2D>();
    }

    public void EnterPortal()
    {
        StartCoroutine(EnterPortalCo());
    }

    IEnumerator EnterPortalCo()
    {
        var player = PlayableCharacters.GetPlayerInScene();
        player.SetIsHumanControlled(false, showChangeEffect: false);
        player.DisableCollider();

        float effectTime = 1.0f;
        float timeEnd = Time.unscaledTime + effectTime;
        Vector3 portalCenter = transform.position + Vector3.up * 1.5f;

        collider_.enabled = false;
        enterSound_.Play();

        var baseScale = player.transform.localScale;

        while (Time.unscaledTime < timeEnd)
        {
            float t = Mathf.Clamp01(1.0f - ((timeEnd - Time.unscaledTime) / effectTime));
            float negT = 1.0f - t;
            cameraShake_.SetMinimumShake(1);

            var playerPos = player.transform.position;
            var direction = portalCenter - playerPos;
            player.SetForce(direction * 20);
            player.transform.localScale = baseScale * negT;
            player.transform.localRotation = Quaternion.Euler(0, 0, t * 360 * 5);

            yield return null;
        }

        OnPlayerEnter?.Invoke();
    }

    void Update()
    {
        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond);
    }
}
