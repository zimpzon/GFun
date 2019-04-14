using UnityEngine;

public interface IMovableActor
{
    Vector3 GetPosition();
    void SetMovementVector(Vector3 vector, bool isNormalized = true);
}
