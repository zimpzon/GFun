using UnityEngine;

public static class MapBuilderSingleRoom
{
    public static void Build(int w, int h)
    {
        MapBuilder.Fillrect(new Vector2Int(25, 25), w, h, 1);
    }
}
