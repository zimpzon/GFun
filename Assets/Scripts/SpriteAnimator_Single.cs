using UnityEngine;

public class SpriteAnimator_Single : MonoBehaviour, ISpriteAnimator
{
    public SpriteAnimationFrames_Single Anim;
    public Sprite SpriteWhenDead;

    SpriteRenderer renderer_;
    float randomOffset_;

    public void UpdateAnimation(Vector3 latestMovementDirection, bool isDead = false)
    {
        if (isDead)
            renderer_.sprite = SpriteWhenDead;
        else
            renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond, randomOffset_);

        renderer_.flipX = latestMovementDirection.x < 0;
    }

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        randomOffset_ = Random.value;
    }
}
