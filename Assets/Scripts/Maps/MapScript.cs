using GFun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapScript : MonoBehaviour, IMapAccess
{
    public static MapScript Instance;

    public Tilemap FloorTileMap;
    public Tilemap WallTileMap;
    public Tilemap TopTileMap;
    public GameObject BackgroundQuad;
    public CompositeCollider2D WallCompositeCollider;
    public TilemapCollider2D TilemapCollider;
    public Color MapColor = Color.white;

    const int ExplosionDamageToEnemies = 40;

    Renderer wallRenderer_;
    Renderer topRenderer_;
    Renderer backgroundRenderer_;
    Renderer floorRenderer_;
    float wallClarity_;
    bool shaderHasClarity_;
    MapStyle mapStyle_;

    static readonly Collider2D[] TempColliderResult = new Collider2D[50];

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

    public void SetMapStyle(MapStyle mapStyle)
    {
        mapStyle_ = mapStyle;
        SetColor(mapStyle_.Color);
    }

    void SetColor(Color color)
    {
        wallRenderer_.material.SetColor("_Color", color);
        floorRenderer_.material.SetColor("_Color", color);
        topRenderer_.material.SetColor("_Color", color);
        backgroundRenderer_.material.SetColor("_Color", color);
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
    public void TriggerExplosion(Vector3 worldPosition, float worldRadius, bool damageWallsOnly = true, IEnemy explosionSource = null, bool damageSelf = true)
    {
        int tilesChecked = MapUtil.ClearCircle(this, mapStyle_, worldPosition, worldRadius);
        var particles = SceneGlobals.Instance.ParticleScript.WallDestructionParticles;

        WallCompositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;

        int tilesCleared = 0;

        for (int i = 0; i < tilesChecked; ++i)
        {
            bool wasDestroyed = MapUtil.LatestResultFlags[i];
            if (wasDestroyed)
                tilesCleared++;

            if (wasDestroyed || !damageWallsOnly)
            {
                var tile = MapUtil.LatestResultPositions[i];

                // Show effect a bit above wall tile center since they also have a top
                const float EffectOffset = 0.5f;
                var tileWorldPos = WallTileMap.GetCellCenterWorld(tile) + Vector3.up * EffectOffset;
                ParticleScript.EmitAtPosition(particles, tileWorldPos, 10);
            }
        }

        if (!damageWallsOnly)
        {
            var player = PlayableCharacters.GetPlayerInScene();
            var playerPos = player.GetPosition();
            float playerDistanceFromExplosion = (worldPosition - playerPos).magnitude;
            bool damagePlayer = playerDistanceFromExplosion < worldRadius * 0.8f;
            if (damagePlayer)
                player.TakeDamage(explosionSource, CurrentRunData.Instance.PlayerExplosionDamage, Vector3.zero);

            int mask = SceneGlobals.Instance.EnemyDeadOrAliveMask;
            int count = Physics2D.OverlapCircleNonAlloc(worldPosition, worldRadius * 0.9f, TempColliderResult, mask);
            for (int i = 0; i< count; ++i)
            {
                var enemyGO = TempColliderResult[i];
                var enemy = enemyGO.GetComponent<IEnemy>();

                bool isSelf = enemy == explosionSource;
                bool skipDueToSelfDamage = isSelf && !damageSelf;
                if (enemy != null && !skipDueToSelfDamage)
                {
                    var diff = enemyGO.transform.position - worldPosition;
                    var direction = diff.normalized;
                    enemy.TakeDamage(ExplosionDamageToEnemies, direction);
                }
            }
        }

        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.LargeExplosion1, 1, 0.1f);
        SceneGlobals.Instance.CameraShake.SetMinimumShake(1.0f);

        WallCompositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
        TilemapCollider.enabled = false;
        TilemapCollider.enabled = true;
    }

    void Awake()
    {
        Instance = this;

        wallRenderer_ = WallTileMap.GetComponent<Renderer>();
        topRenderer_ = TopTileMap.GetComponent<Renderer>();
        backgroundRenderer_ = BackgroundQuad?.GetComponent<Renderer>();
        floorRenderer_ = FloorTileMap.GetComponent<Renderer>();

        // Wall, top and background should all be equal so just pick one to start with
        shaderHasClarity_ = topRenderer_.material.HasProperty("_Clarity");
        SetWallClarity(0.9f);

        wallClarity_ = shaderHasClarity_ ? -topRenderer_.material.GetFloat("_Clarity") : 0.0f;
        SetColor(MapColor);

        TouchMap();
    }

    public int GetCollisionTileValue(Vector3 worldPos, int valueIfOutsideBounds = 1)
        => GetCollisionTileValue((int)worldPos.x, (int)worldPos.y, valueIfOutsideBounds);

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

    public Vector3 GetTileBottomMid(Vector3 worldPos)
        => new Vector3((int)worldPos.x + 0.5f, (int)worldPos.y);

    public Vector2Int GetCollisionTilePosFromWorldPos(Vector3 worldPos)
        => new Vector2Int((int)worldPos.x, (int)worldPos.y);

    public Vector3 GetWorldPosFromCollisionTileCenter(int tileX, int tileY)
        => new Vector3(tileX + 0.5f, tileY + 0.5f);

    public void BuildCollisionMapFromFloorTilemap(Tilemap floorTilemap)
        => MapBuilder.BuildCollisionMapFromFloorTilemap(floorTilemap);

    public void BuildCollisionMapFromWallTilemap(Tilemap wallTilemap)
        => MapBuilder.BuildCollisionMapFromWallTilemap(wallTilemap);
}
