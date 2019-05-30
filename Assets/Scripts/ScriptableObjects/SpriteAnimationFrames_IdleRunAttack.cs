using UnityEngine;

[CreateAssetMenu(fileName = "new SpriteAnimationFrames_IdleRunAttack.asset", menuName = "GFun/Sprite Frames, IdleRunAttack", order = 20)]
public class SpriteAnimationFrames_IdleRunAttack : ScriptableObject
{
    public float DefaultAnimationFramesPerSecond = 6;
    public Sprite[] Idle;
    public Sprite[] Run;
    public Sprite[] Attack;
}
