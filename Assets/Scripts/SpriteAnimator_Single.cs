using UnityEngine;

public class SpriteAnimator_Single : MonoBehaviour, ISpriteAnimator
{
    public SpriteAnimationFrames_Single Anim;

    SpriteRenderer renderer_;
    float randomOffset_;

    public void UpdateAnimation(Vector3 latestMovementDirection)
    {
        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond, randomOffset_);
    }

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        randomOffset_ = Random.value;
    }
}
