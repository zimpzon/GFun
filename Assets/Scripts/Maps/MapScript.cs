using UnityEngine;
using UnityEngine.Tilemaps;

public class MapScript : MonoBehaviour
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

    public float GetWallClarity()
        => wallClarity_;

    public void SetWallClarity(float value01)
    {
        wallClarity_ = Mathf.Clamp01(value01);

        wallRenderer_.material.SetFloat("_ClarityTop", wallClarity_);
        wallRenderer_.material.SetFloat("_ClarityBottom", wallClarity_ * 2); // Bottom wall is less affected
        topRenderer_.material.SetFloat("_Clarity", wallClarity_);
        backgroundRenderer_.material.SetFloat("_Clarity", wallClarity_);
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
                particles.transform.position = tileWorldPos;
                particles.Emit(10);
                SceneGlobals.Instance.CameraShake.AddShake(4.0f);
                tilesCleared++;
            }
        }

        WallCompositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
    }

    void Awake()
    {
        wallRenderer_ = WallTileMap.GetComponent<Renderer>();
        topRenderer_ = TopTileMap.GetComponent<Renderer>();
        backgroundRenderer_ = BackgroundQuad.GetComponent<Renderer>();

        // Wall, top and background should all be equal so just pick one to start with
        wallClarity_ = -topRenderer_.material.GetFloat("_Clarity");

        TouchMap();
    }
}
