using UnityEngine;

public enum PlayerPositionType { Tile, Center, Feet };

public interface ISensingActor
{
    void LookForPlayerLoS(bool enabled, float maxDistance);
    float GetPlayerLatestKnownPositionAge();
    Vector3 GetPlayerLatestKnownPosition(PlayerPositionType type);
}
