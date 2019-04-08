using UnityEngine;

public static class MapBuilderSingleRoom
{
    public static void Build(int w, int h)
    {
        MapBuilder.Fillrect(new Vector2Int(MapBuilder.MapMaxWidth / 2, MapBuilder.MapMaxHeight / 2), w, h, 1);
    }
}
