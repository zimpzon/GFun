using UnityEngine;

[CreateAssetMenu(fileName = "new SpriteAnimationFrames_IdleRun.asset", menuName = "GFun/Sprite Frames, IdleRun", order = 20)]
public class SpriteAnimationFrames_IdleRun : ScriptableObject
{
    public float DefaultAnimationFramesPerSecond = 6;
    public Sprite[] Idle;
    public Sprite[] Run;
}
