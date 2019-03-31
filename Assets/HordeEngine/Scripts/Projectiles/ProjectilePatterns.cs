using System.Collections.Generic;
using UnityEngine;

namespace HordeEngine
{
    public static class ProjectilePatterns
    {
        public static IEnumerable<Vector2> PatternPositions(string[] pattern, float itemSize)
        {
            Vector2 pos = Vector2.zero;
            for (int y = 0; y < pattern.Length; ++y)
            {
                pos.y = y * itemSize;
                pos.x = 0.0f;
                for (int x = 0; x < pattern[y].Length; ++x)
                {
                    if (pattern[y][x] != ' ')
                    {
                        yield return pos;
                    }
                    pos.x += itemSize;
                }
            }
        }

        public static string[] Test = new string[]
        {
            "1   1",
            " 1 1 ",
            "  1  ",
            " 1 1 ",
            "1   1",
        };
    }
}
