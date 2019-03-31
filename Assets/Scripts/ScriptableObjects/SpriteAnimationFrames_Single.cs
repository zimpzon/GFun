using UnityEngine;

[CreateAssetMenu(fileName = "new SpriteAnimationFrames_Single.asset", menuName = "GFun/Sprite Frames, Single", order = 20)]
public class SpriteAnimationFrames_Single : ScriptableObject
{
    public float DefaultAnimationFramesPerSecond = 6;
    public Sprite[] Sprites;
}
