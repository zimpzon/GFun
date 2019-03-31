using UnityEngine;

[CreateAssetMenu(fileName = "new SpriteAnimationFrames_IdleRun.asset", menuName = "HordeEngine/Sprite Frames, IdleRun", order = 20)]
public class SpriteAnimationFrames_IdleRun : ScriptableObject
{
    public float DefaultAnimationFramesPerSecond;
    public Sprite[] Idle;
    public Sprite[] Run;
}
