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
        if(Time.time > nextMove_)
        {
            nextMove_ = Time.time + Random.value * 2 + 1;
            dir_ = Random.insideUnitCircle;
        }
        movable_.SetMovementVector(dir_);
    }
}
