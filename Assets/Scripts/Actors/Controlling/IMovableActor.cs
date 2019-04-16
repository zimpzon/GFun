using UnityEngine;

public interface IMovableActor
{
    /// <summary>
    /// Is the actor dead?
    /// </summary>
    bool IsDead();

    float GetSpeed();
    void SetSpeed(float speed);

    Vector3 GetPosition();

    /// <summary>
    /// Actor will try to move to destination
    /// </summary>
    void MoveTo(Vector3 destination);

    /// <summary>
    /// Cancels destination set with MoveTo
    /// </summary>
    void StopMove();

    /// <summary>
    /// Was target set with MoveTo reached?
    /// </summary>
    bool MoveTargetReached();
}
