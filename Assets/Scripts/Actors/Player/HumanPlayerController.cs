using UnityEngine;

public class HumanPlayerController
{
    IMovableActor movable_;

    // Abilities? Shoot (arrows), time (space), active ability (q), reload (r), interact (e), bomb (f)
    public void TakeControl(object actor)
    {
        movable_ = (IMovableActor)actor;
    }

    void Update()
    {
        var horz = Input.GetAxisRaw("Horizontal");
        var vert = Input.GetAxisRaw("Vertical");
        var moveVec = new Vector3(horz, vert);
    }
}
