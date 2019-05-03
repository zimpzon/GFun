using UnityEngine;

public interface IPhysicsActor
{
    void SetMinimumForce(Vector3 force);
    void AddForce(Vector3 force);
}
