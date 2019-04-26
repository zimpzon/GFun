using Apex.Examples.AI;
using Apex.Examples.AI.Game;

public class FleeingBatController : EntityComponentBase
{
    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;

    public override EntityType AiType => EntityType.FleeingBat;

    private new void Awake()
    {
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.SetLookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();

        base.Awake();
    }
}
