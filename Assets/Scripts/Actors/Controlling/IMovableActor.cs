using UnityEngine;

public interface IMovableActor
{
    float GetSpeed();
    void SetSpeed(float speed);

    /// <summary>
    /// Position where actor is anchored to the floor (typically feet)
    /// </summary>
    Vector3 GetPosition();

    /// <summary>
    /// Center of the actor
    /// </summary>
    Vector3 GetCenter();

    /// <summary>
    /// Actor will try to move to destination
    /// </summary>
    void MoveTo(Vector3 destination);

    Vector3 GetMoveDestination();

    /// <summary>
    /// Cancels destination set with MoveTo
    /// </summary>
    void StopMove();

    /// <summary>
    /// Was target set with MoveTo reached?
    /// </summary>
    bool MoveTargetReached();
}
