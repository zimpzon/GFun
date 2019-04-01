using UnityEngine;

public class AttackTotemScript : MonoBehaviour
{
    public SpriteAnimationFrames_Single Anim;
    SpriteRenderer renderer_;

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // TODO: flipX from player X
        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond);
    }
}
