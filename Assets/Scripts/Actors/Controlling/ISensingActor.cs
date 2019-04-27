using UnityEngine;

public enum PlayerPositionType { Tile, Center, Feet };

public interface ISensingActor
{
    void SetLookForPlayerLoS(bool enabled, float maxDistance);
    float GetPlayerLatestKnownPositionAge();
    Vector3 GetPlayerLatestKnownPosition(PlayerPositionType type);

    void SetLookForNearbyCover(bool enabled, float maxDistance);
    bool HasNearbyCover { get; }
    Vector3 NearbyCoverPosition { get; }
}
