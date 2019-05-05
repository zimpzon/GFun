using UnityEngine;

public interface IPhysicsActor
{
    void SetMinimumForce(Vector3 force);
    void AddForce(Vector3 force, ForceMode2D forceMode = ForceMode2D.Impulse);
}
