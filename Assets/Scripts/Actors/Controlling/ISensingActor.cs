using UnityEngine;

public interface ISensingActor
{
    void SetCheckPlayerLoS(bool doCheck);
    float GetPlayerLoSAge();
    Vector3 GetPlayerLatestKnownPosition();
}
