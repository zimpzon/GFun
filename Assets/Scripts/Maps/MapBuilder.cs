using UnityEngine;
using UnityEngine.Tilemaps;

public enum MapFloorAlgorithm { SingleRoom, RandomWalkers, CaveLike1,  }

public static class MapBuilder
{
    public const int MapMaxWidth = 100;
    public const int MapMaxHeight = 100;
    public static readonly RectInt Rect = new RectInt(0, 0, MapMaxWidth, MapMaxHeight);
    public static readonly Vector2Int Center = new Vector2Int((int)Rect.center.x, (int)Rect.center.y);

    public static byte[,] map = new byte[MapMaxWidth, MapMaxHeight];

    public static void ApplyFloorTiles(MapScript mapScript, MapStyle mapStyle)
    {
        Vector3Int pos = Vector3Int.zero;

        for (int y = 0; y < Rect.height; ++y)
        {
            for (int x = 0; x < Rect.width; ++x)
            {
                if (map[x, y] != 0)
                {
                    pos.x = x;
                    pos.y = y;
                    var tile = ChooseTile(mapStyle.FloorTile, mapStyle.FloorTileVariations, mapStyle.FloorVariationChance);
                    mapScript.FloorTileMap.SetTile(pos, tile);
                }
            }
        }
        mapScript.FloorTileMap.CompressBounds();
    }

    public static void BuildWallTiles(MapScript mapScript, MapStyle mapStyle)
    {
        var bounds = mapScript.FloorTileMap.cellBounds;
        var pos = Vector3Int.zero;

        for (int y = bounds.yMin; y <= bounds.yMax; ++y)
        {
            for (int x = bounds.xMin; x <= bounds.xMax; ++x)
            {
                pos.x = x;
                pos.y = y;

                var floorCurrent = mapScript.FloorTileMap.GetTile(pos);
                if (floorCurrent != null)
                    SingleFloorTileBuildWalls(mapScript, mapStyle, pos);
            }
        }
    }

    static void SingleFloorTileBuildWalls(MapScript mapScript, MapStyle mapStyle, Vector3Int pos)
    {
        // If below, left or right is empty, add an invisible wall tile in wall layer there (for collision only)
        // If above is empty, add a visible wall-tile above in wall layer
        var posLeft = pos + Vector3Int.left;
        var posRight = pos + Vector3Int.right;
        var posUp = pos + Vector3Int.up;
        var posDown = pos + Vector3Int.down;

        var floorLeft = mapScript.FloorTileMap.GetTile(posLeft);
        var floorRight = mapScript.FloorTileMap.GetTile(posRight);
        var floorUp = mapScript.FloorTileMap.GetTile(posUp);
        var floorDown = mapScript.FloorTileMap.GetTile(posDown);

        var wallLeft = mapScript.WallTileMap.GetTile(posLeft);
        var wallRight = mapScript.WallTileMap.GetTile(posRight);
        var wallUp = mapScript.WallTileMap.GetTile(posUp);
        var wallDown = mapScript.WallTileMap.GetTile(posDown);

        // Invisible walls
        if (floorLeft == null && wallLeft == null)
            mapScript.WallTileMap.SetTile(posLeft, mapStyle.InvislbleWallTile);
        if (floorRight == null && wallRight == null)
            mapScript.WallTileMap.SetTile(posRight, mapStyle.InvislbleWallTile);
        if (floorDown == null && wallDown == null)
            mapScript.WallTileMap.SetTile(posDown, mapStyle.InvislbleWallTile);

        // Visible wall
        if (floorUp == null)
        {
            var wallTile = ChooseTile(mapStyle.WallTile, mapStyle.WallTileVariations, mapStyle.WallVariationChance);
            mapScript.WallTileMap.SetTile(posUp, wallTile);
        }

        // Top layer
        if (floorDown == null)
        {
            var topTile = ChooseTile(mapStyle.TopTile, mapStyle.TopTileVariations, mapStyle.TopVariationChance);
            mapScript.TopTileMap.SetTile(pos, topTile);
        }
    }

    public static void DestroyTile(MapScript mapScript, MapStyle mapStyle, Vector3Int pos)
    {
        // Clear wall
        mapScript.WallTileMap.SetTile(pos, null);

        // Clear top
        var topPos = pos + Vector3Int.up;
        mapScript.TopTileMap.SetTile(topPos, null);

        // Add floor
        var floorTile = ChooseTile(mapStyle.FloorTile, mapStyle.FloorTileVariations, mapStyle.FloorVariationChance);
        mapScript.FloorTileMap.SetTile(pos, floorTile);

        SingleFloorTileBuildWalls(mapScript, mapStyle, pos);
    }

    static TileBase ChooseTile(TileBase tile, TileBase[] variations, float variationChance)
    {
        int variationCount = variations.Length;
        if (variationCount == 0 || Random.value > variationChance)
            return tile;

        return variations[Random.Range(0, variationCount)];
    }

    public static void GenerateMapFloor(int w, int h, MapFloorAlgorithm algo)
    {
        ZeroMap();

        if (algo == MapFloorAlgorithm.SingleRoom)
            MapBuilderSingleRoom.Build(w, h);

        else if (algo == MapFloorAlgorithm.RandomWalkers)
            MapBuilderRandomWalkers.Build(w, h);

        else if (algo == MapFloorAlgorithm.CaveLike1)
            MapBuilderCaveLike1.Build(w, h);

        else
            SceneGlobals.Instance.DebugLinesScript.SetLine("Unknown map algorithm", algo);
    }

    static RectInt CreateRect(int x, int y, int w, int h) => new RectInt(x - w / 2, y - h / 2, w, h);
    static RectInt CreateRect(Vector2Int center, int w, int h) => new RectInt(center.x - w / 2, center.y - h / 2, w, h);

    public static void Fillrect(int x, int y, int w, int h, byte value) => Fillrect(CreateRect(x, y, w, h), value);
    public static void Fillrect(Vector2Int center, int w, int h, byte value) => Fillrect(CreateRect(center.x, center.y, w, h), value);
    public static void Fillrect(RectInt rect, byte value)
    {
        foreach(var pos in rect.allPositionsWithin)
        {
            map[pos.x, pos.y] = value;
        }
    }

    static void ZeroMap()
        => Fillrect(Rect, 0);
}
