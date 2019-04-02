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

    // When destroy walls some colliders are left behind. Fix should be incoming:
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
        //WallCompositeCollider = WallTileMap.GetComponent<CompositeCollider2D>();
        //WallCompositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
    }
}
