using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class MapPluginScript : MonoBehaviour
{
    public virtual string Name { get; } = "<name>";
    public BoundsInt AppliedCellBounds;

    public abstract void ApplyToMap(Vector3Int position);
    public abstract Vector3 GetPlayerStartPosition();

    private void Awake()
    {
        var tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        if (tilemapRenderer != null)
            tilemapRenderer.enabled = false;
    }

    public virtual IEnumerator<float> GameLoopCo()
    {
        yield return 0;
    }

    protected void ApplyTilemap(Vector3Int position)
    {
        var tilemap = GetComponentInChildren<Tilemap>();
        foreach (var tilePos in tilemap.cellBounds.allPositionsWithin)
        {
            var worldPosition = tilePos + position;
            if (tilemap.HasTile(tilePos))
                MapBuilder.MarkAsFloor(worldPosition);
        }

        transform.position = position;
        AppliedCellBounds = new BoundsInt(tilemap.cellBounds.position + position, tilemap.cellBounds.size);
    }
}
