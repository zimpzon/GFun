using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy1 : MonoBehaviour
{
    public SpriteAnimationFrames_Single Anim;

    SpriteRenderer renderer_;

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(Anim.Sprites, Anim.DefaultAnimationFramesPerSecond);
    }
}
