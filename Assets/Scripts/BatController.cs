using UnityEngine;

public class BatController : MonoBehaviour
{
    IMovableActor movable_;
    float nextMove_;
    Vector3 dir_;

    void Start()
    {
        movable_ = GetComponent<IMovableActor>();
    }

    void Update()
    {
        // TODO:
        // if recent player LoS set move target
        //    if reached, clear player LoS
        // else if has no target
        //    set target to random close position in sight
        // if target reached clear target
        if(Time.time > nextMove_)
        {
            nextMove_ = Time.time + Random.value * 2 + 1;
            dir_ = Random.insideUnitCircle;
        }
        movable_.SetMovementVector(dir_);
    }
}
