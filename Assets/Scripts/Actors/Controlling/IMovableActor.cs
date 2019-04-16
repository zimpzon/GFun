using UnityEngine;

public interface IMovableActor
{
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
