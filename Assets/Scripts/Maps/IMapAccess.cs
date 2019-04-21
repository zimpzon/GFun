using UnityEngine;
using UnityEngine.Tilemaps;

namespace GFun
{
    public interface IMapAccess
    {
        /// <summary>
        /// Get center of the tile containing worldPos
        /// </summary>
        Vector3 GetTileBottomMid(Vector3 worldPos);

        /// <summary>
        /// 0 = walkable
        /// > 0 = not walkable
        /// </summary>
        int GetCollisionTileValue(int tileX, int tileY, int valueIfOutsideBounds = 1);
        int GetCollisionTileValue(Vector3 worldPos, int valueIfOutsideBounds = 1);

        Vector2Int GetCollisionTilePosFromWorldPos(Vector3 worldPos);

        /// <summary>
        /// Returns the world position of the center of the cell at cellX, cellY
        /// </summary>
        Vector3 GetWorldPosFromCollisionTileCenter(int tileX, int tileY);

        /// <summary>
        /// Scans floorTileMap and updates the collision map.
        /// </summary>
        void BuildCollisionMapFromFloorTilemap(Tilemap floorTilemap);

        /// <summary>
        /// Destroy tiles at worldPosition in a world radius of worldRadius
        /// </summary>
        void ExplodeWalls(Vector3 worldPosition, float worldRadius, bool particlesAtDestroyedWallsOnly = true);

        /// <summary>
        /// Sets how much the map walls should be affected by the lighting effect (0..1)
        /// </summary>
        void SetWallClarity(float value01);
    }
}
