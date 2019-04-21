using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    public Sprite[] AnimationSprites;
    public float AnimationFramesPerSecond = 5.0f;

    float randomOffset_;

    SpriteRenderer renderer_;

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        randomOffset_ = Random.value * AnimationSprites.Length;
    }

    private void Update()
    {
        renderer_.sprite = GetAnimationSprite(AnimationSprites, AnimationFramesPerSecond, randomOffset_);
    }

    public static Sprite GetAnimationSprite(Sprite[] sprites, float animationFramesPerSecond, float offset01 = 0)
    {
        int id = (int)(Time.unscaledTime * animationFramesPerSecond + offset01 * sprites.Length) % sprites.Length;
        return sprites[id];
    }

    public static Rect GetAnimationUvRect(Sprite[] sprites, float animationFramesPerSecond)
    {
        Sprite spr = GetAnimationSprite(sprites, animationFramesPerSecond);
        return spr.textureRect;
    }
}
