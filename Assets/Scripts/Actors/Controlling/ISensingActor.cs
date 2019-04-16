using UnityEngine;

public interface ISensingActor
{
    void LookForPlayerLoS(bool enabled, float maxDistance);
    float GetPlayerLatestKnownPositionAge();
    Vector3 GetPlayerLatestKnownPosition();
}
