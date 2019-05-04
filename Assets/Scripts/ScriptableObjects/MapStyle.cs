using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "new MapStyle.asset", menuName = "GFun/Map Style", order = 10)]
public class MapStyle : ScriptableObject
{
    [Header("Floor")]
    public TileBase FloorTile;
    public TileBase[] FloorTileVariations;
    public float FloorVariationChance = 0.1f;

    [Header("Wall")]
    public TileBase WallTile;
    public TileBase[] WallTileVariations;
    public float WallVariationChance = 0.1f;
    public TileBase InvisibleWallTile;

    [Header("Top")]
    public TileBase TopTile;
    public TileBase[] TopTileVariations;
    public float TopVariationChance = 0.1f;
}
