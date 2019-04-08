using GFun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapScript : MonoBehaviour, IMapAccess
{
    public Tilemap FloorTileMap;
    public Tilemap WallTileMap;
    public Tilemap TopTileMap;
    public MapStyle MapStyle;
    public GameObject BackgroundQuad;
    public CompositeCollider2D WallCompositeCollider;

    Renderer wallRenderer_;
    Renderer topRenderer_;
    Renderer backgroundRenderer_;
    float wallClarity_;
    bool shaderHasClarity_;

    public float GetWallClarity()
        => wallClarity_;

    public void SetWallClarity(float value01)
    {
        if (shaderHasClarity_)
        {
            wallClarity_ = Mathf.Clamp01(value01);

            wallRenderer_.material.SetFloat("_ClarityTop", wallClarity_);
            wallRenderer_.material.SetFloat("_ClarityBottom", wallClarity_ * 2); // Bottom wall is less affected
            topRenderer_.material.SetFloat("_Clarity", wallClarity_);
            backgroundRenderer_.material.SetFloat("_Clarity", wallClarity_);
        }
    }

    /// <summary>
    /// Access cells at start to avoid spike mid-game the first time it is accessed
    /// </summary>
    void TouchMap()
    {
        // TODO: Currently not helping, touch the right places.
        var wallbounds = WallTileMap.cellBounds;
        var topbounds = WallTileMap.cellBounds;
        var floorbounds = WallTileMap.cellBounds;
    }

    // When destroying walls some colliders are left behind. Fix should be incoming:
    // https://github.com/Unity-Technologies/2d-extras/issues/34
    public void ExplodeWalls(Vector3 worldPosition, float worldRadius)
    {
        int tilesChecked = MapUtil.ClearCircle(this, MapStyle, worldPosition, worldRadius);
        var particles = SceneGlobals.Instance.ParticleScript.WallDestructionParticles;

        WallCompositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;

        int tilesCleared = 0;
        for (int i = 0; i < tilesChecked; ++i)
        {
            if (MapUtil.LatestResultFlags[i])
            {
                var tile = MapUtil.LatestResultPositions[i];
                // Show effect a bit above wall tile center since they also have a top
                const float EffectOffset = 0.5f;
                var tileWorldPos = WallTileMap.GetCellCenterWorld(tile) + Vector3.up * EffectOffset;
                ParticleScript.EmitAtPosition(particles, tileWorldPos, 10);
                tilesCleared++;
            }
        }

        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.LargeExplosion1, 1, 0.1f);
        SceneGlobals.Instance.CameraShake.SetMinimumShake(1.0f);

        WallCompositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
    }

    void Awake()
    {
        wallRenderer_ = WallTileMap.GetComponent<Renderer>();
        topRenderer_ = TopTileMap.GetComponent<Renderer>();
        backgroundRenderer_ = BackgroundQuad?.GetComponent<Renderer>();

        // Wall, top and background should all be equal so just pick one to start with
        shaderHasClarity_ = topRenderer_.material.HasProperty("_Clarity");
        wallClarity_ = shaderHasClarity_ ? -topRenderer_.material.GetFloat("_Clarity") : 0.0f;

        TouchMap();
    }

    public int GetCollisionTileValue(int tileX, int tileY, int valueIfOutsideBounds = 1)
    {
        if (tileX < 0 || tileX >= MapBuilder.MapMaxWidth || tileY < 0 || tileY >= MapBuilder.MapMaxHeight)
            return valueIfOutsideBounds;

        return MapBuilder.CollisionMap[tileX, tileY];
    }

    public void DebugDrawCollisionTile(int tileX, int tileY, bool dark)
    {
        var worldPos = GetWorldPosFromCollisionTileCenter(tileX, tileY);
        int value = GetCollisionTileValue(tileX, tileY, -1);
        var col = Color.red;
        if (value == -1)
            col = Color.yellow;
        if (value == MapBuilder.TileWalkable)
            col = Color.green;

        if (dark)
            col *= 0.5f;

        Debug.DrawRay(worldPos, Vector3.down * 0.25f, col);
        Debug.DrawRay(worldPos, Vector3.up * 0.25f, col);
        Debug.DrawRay(worldPos, Vector3.left * 0.25f, col);
        Debug.DrawRay(worldPos, Vector3.right * 0.25f, col);
    }

    public Vector2Int GetCollisionTilePosFromWorldPos(Vector3 worldPos)
        => new Vector2Int((int)worldPos.x, (int)worldPos.y);

    public Vector3 GetWorldPosFromCollisionTileCenter(int tileX, int tileY)
        => new Vector3(tileX + 0.5f, tileY + 0.5f);

    public void BuildCollisionMapFromFloorTilemap(Tilemap floorTilemap)
        => MapBuilder.BuildCollisionMapFromFloorTilemap(floorTilemap);
}
