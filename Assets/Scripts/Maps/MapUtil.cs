using System.Collections.Generic;
using UnityEngine;

public static class MapUtil
{
    // Reuse the same lists for all returned results. Assumes single-threaded.
    public static List<Vector3Int> LatestResultPositions = new List<Vector3Int>(50);
    public static List<bool> LatestResultFlags = new List<bool>(50);

    /// <summary>
    /// Clears cells in tilemap in a circle at worldPosition within worldRadius.
    /// Returns number of cleared cells. LatestResult contains the cells.
    /// </summary>
    public static int ClearCircle(MapScript mapScript, MapStyle mapStyle, Vector3 worldPosition, float worldRadius)
    {
        LatestResultPositions.Clear();
        LatestResultFlags.Clear();

        var wallTiles = mapScript.WallTileMap;
        var cellCenter = wallTiles.WorldToCell(worldPosition);
        int cellRadius = (int)(worldRadius / wallTiles.cellSize.x); // Assuming square tiles

        int y0 = cellCenter.y - cellRadius;
        int y1 = cellCenter.y + cellRadius;
        int x0 = cellCenter.x - cellRadius;
        int x1 = cellCenter.x + cellRadius;

        var cellPos = new Vector3Int();
        for (int y = y0; y <= y1; ++y)
        {
            for (int x = x0; x <= x1; ++x)
            {
                cellPos.x = x;
                cellPos.y = y;
                var offsetFromCenter = cellCenter - cellPos;
                if (offsetFromCenter.sqrMagnitude < cellRadius)
                {
                    bool hasTile = wallTiles.HasTile(cellPos);
                    LatestResultPositions.Add(cellPos);
                    LatestResultFlags.Add(hasTile);

                    if (hasTile)
                        MapBuilder.DestroyTile(mapScript, mapStyle, cellPos);
                }
            }
        }

        return LatestResultPositions.Count;
    }

    public static Vector3 GetLeftmostFreeCell()
    {
        for (int y = 0; y < MapBuilder.MapMaxHeight; ++y)
        {
            for (int x = 0; x < MapBuilder.MapMaxWidth; ++x)
            {
                if (MapBuilder.CollisionMap[x, y] == MapBuilder.TileWalkable)
                    return new Vector3(x + 0.5f, y + 0.5f);
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetmostFreeCell()
    {
        for (int y = 0; y < MapBuilder.MapMaxHeight; ++y)
        {
            for (int x = 0; x < MapBuilder.MapMaxWidth; ++x)
            {
                if (MapBuilder.CollisionMap[x, y] == MapBuilder.TileWalkable)
                    return new Vector3(x + 0.5f, y + 0.5f);
            }
        }

        return Vector3.zero;
    }
}
