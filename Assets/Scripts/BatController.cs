using System.Collections;
using UnityEngine;

public class BatController : MonoBehaviour
{
    IMovableActor movable_;
    ISensingActor senses_;
    Vector3 dir_;

    void Start()
    {
        movable_ = GetComponent<IMovableActor>();
        senses_ = GetComponent<ISensingActor>();
        senses_.LookForPlayerLoS(true, 10);

        StartCoroutine(AI());
    }

    static readonly WaitForSeconds Wait = new WaitForSeconds(1.0f);

    IEnumerator AI()
    {
        while (true)
        {
            yield return Wait;
            var myPos = movable_.GetPosition();
            var target = senses_.GetPlayerLatestKnownPosition();
            movable_.MoveTo(target);
        }
    }
}
