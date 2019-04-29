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

        var graph = AstarPath.active.data.gridGraph;

        var cellPos = new Vector3Int();
        for (int y = y0; y <= y1; ++y)
        {
            for (int x = x0; x <= x1; ++x)
            {
                cellPos.x = x;
                cellPos.y = y;
                var offsetFromCenter = cellCenter - cellPos;
                if (offsetFromCenter.magnitude < cellRadius)
                {
                    bool hasTile = wallTiles.HasTile(cellPos);
                    LatestResultPositions.Add(cellPos);
                    LatestResultFlags.Add(hasTile);

                    if (hasTile)
                    {
                        MapBuilder.DestroyTile(mapScript, mapStyle, cellPos);
                        var node = graph.GetNode(x, y);
                        node.Walkable = true;
                        graph.CalculateConnections(x + 0, y + 0);
                        graph.CalculateConnections(x + 1, y + 0);
                        graph.CalculateConnections(x + 0, y + 1);
                        graph.CalculateConnections(x - 1, y + 0);
                        graph.CalculateConnections(x + 0, y - 1);
                    }
                }
            }
        }
        return LatestResultPositions.Count;
    }

    public static int GetCollisionValue(Vector3 pos)
        => MapBuilder.CollisionMap[(int)pos.x, (int)pos.y];

    public static Vector3 GetRandomEdgePosition(out Vector3 directionToMapCenter)
    {
        float rnd = Random.value;
        if (rnd < 0.25f)
        {
            directionToMapCenter = Vector3.right;
            return GetLeftmostFreeCell();
        }

        if (rnd < 0.5f)
        {
            directionToMapCenter = Vector3.down;
            return GetTopmostFreeCell();
        }

        if (rnd < 0.75f)
        {
            directionToMapCenter = Vector3.left;
            return GetRightmostFreeCell();
        }

        directionToMapCenter = Vector3.up;
        return GetBottommostFreeCell();
    }

    public static Vector3 GetLeftmostFreeCell()
    {
        for (int x = 0; x < MapBuilder.MapMaxWidth; ++x)
        {
            for (int y = 0; y < MapBuilder.MapMaxHeight; ++y)
            {
                if (MapBuilder.CollisionMap[x, y] == MapBuilder.TileWalkable)
                    return new Vector3(x + 0.5f, y + 0.5f);
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRightmostFreeCell()
    {
        for (int x = MapBuilder.MapMaxWidth - 1; x > 0; --x)
        {
            for (int y = 0; y < MapBuilder.MapMaxHeight; ++y)
            {
                if (MapBuilder.CollisionMap[x, y] == MapBuilder.TileWalkable)
                    return new Vector3(x + 0.5f, y + 0.5f);
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetBottommostFreeCell()
    {
        for (int y = 0; y < MapBuilder.MapMaxHeight; ++y)
        {
            for (int x = MapBuilder.MapMaxWidth - 1; x > 0; --x)
            {
                if (MapBuilder.CollisionMap[x, y] == MapBuilder.TileWalkable)
                    return new Vector3(x + 0.5f, y + 0.5f);
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetTopmostFreeCell()
    {
        for (int y = MapBuilder.MapMaxHeight - 1; y > 0 ; --y)
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
