using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "new MapStyle.asset", menuName = "GFun/Map Style", order = 10)]
public class MapStyle : ScriptableObject
{
    public Color Color;
    public LightingEffectSettings LightingSettings;

    [Header("Floor")]
    public TileBase FloorTile;
    public TileBase[] FloorTileVariations;
    public float FloorVariationChance = 0.1f;

    [Header("Floor when wall is destroyed")]
    public TileBase FloorBehindWallTile;

    [Header("Wall")]
    public TileBase WallTile;
    public TileBase[] WallTileVariations;
    public float WallVariationChance = 0.1f;
    public TileBase InvisibleWallTile;

    [Header("Top")]
    public TileBase TopTile;
    public TileBase[] TopTileVariations;
    public float TopVariationChance = 0.1f;

    [Header("Roof")]
    public Material BackgroundMaterial;
}
