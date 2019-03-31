using UnityEngine;

namespace HordeEngine
{
    public class AiBlackboard
    {
        // FrameSkip = how often to check line of sight. Every X frames.
        // FrameSkipOffset = offset the categories so its even more spread out
        public const int LineOfSightFrameSkip = 5;
        public const int LineOfSightFrameSkipOffset = 0;

        public Vector2 PlayerPos;
    }
}
