using System.Collections.Generic;
using UnityEngine;

public static class CollisionUtil
{
    public static List<Vector3> LatestResult = new List<Vector3>(10);

    public static void Scan4Directions(Vector3 start)
    {
        CountFreeTiles(start, out int freeUp, out int freeDown, out int freeLeft, out int freeRight);
        LatestResult.Clear();
        LatestResult.Add(Vector3.up * freeUp);
        LatestResult.Add(Vector3.down * freeDown);
        LatestResult.Add(Vector3.left * freeLeft);
        LatestResult.Add(Vector3.right * freeRight);
    }

    public static void RemoveZeroVectorsFromLatestResult()
    {
        for (int i = LatestResult.Count - 1; i >= 0; i--)
        {
            if (LatestResult[i].sqrMagnitude == 0)
                LatestResult.RemoveAt(i);
        }
    }

    public static void RemoveDirectionFromLatestResult(Vector3 direction)
    {
        for (int i = LatestResult.Count - 1; i >= 0; i--)
        {
            if (LatestResult[i].normalized == direction)
                LatestResult.RemoveAt(i);
        }
    }

    public static Vector3 GetRandomFreeDirectionExcept(Vector3 start, Vector3 excludeDirection)
    {
        Scan4Directions(start);
        RemoveZeroVectorsFromLatestResult();
        RemoveDirectionFromLatestResult(excludeDirection);
        if (LatestResult.Count == 0)
            return Vector3.zero;
        return LatestResult[Random.Range(0, LatestResult.Count)];
    }

    public static Vector3 GetRandomFreeDirection(Vector3 start)
        => GetRandomFreeDirectionExcept(start, Vector3.zero);

    public static void CountFreeTiles(Vector3 start, out int up, out int down, out int left, out int right, int max = 8)
    {
        var tiles = MapBuilder.CollisionMap;

        int startX = (int)start.x;
        int startY = (int)start.y;
        if (startX < 0 || startX >= MapBuilder.MapMaxWidth)
            throw new System.ArgumentException($"x position ({startX}) is out of bounds");
        if (startY < 0 || startY >= MapBuilder.MapMaxHeight)
            throw new System.ArgumentException($"x position ({startY}) is out of bounds");

        up = 0;
        down = 0;
        left = 0;
        right = 0;

        while (tiles[startX, startY + up + 1] == MapBuilder.TileWalkable && up < max)
            up++;

        while (tiles[startX, startY - down - 1] == MapBuilder.TileWalkable && down < max)
            down++;

        while (tiles[startX - left - 1, startY] == MapBuilder.TileWalkable && left < max)
            left++;

        while (tiles[startX + right + 1, startY] == MapBuilder.TileWalkable && right < max)
            right++;
    }
}
