using UnityEngine;

public class SpriteAnimator_IdleRun : MonoBehaviour, ISpriteAnimator
{
    public SpriteAnimationFrames_IdleRun Anim;
    public Sprite SpriteWhenDead;

    SpriteRenderer renderer_;
    float randomOffset_;
    bool latestFlip_;

    public void UpdateAnimation(Vector3 latestMovementDirection, bool isDead = false)
    {
        if (isDead)
        {
            renderer_.sprite = SpriteWhenDead;
        }
        else
        {
            bool isRunning = latestMovementDirection != Vector3.zero;
            renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond, randomOffset_);
        }

        if (latestMovementDirection.x == 0)
        {
            renderer_.flipX = latestFlip_;
        }
        else
        {
            renderer_.flipX = latestMovementDirection.x < 0;
            latestFlip_ = renderer_.flipX;
        }
    }

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        randomOffset_ = Random.value;
    }
}
