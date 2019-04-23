using Apex.Examples.AI;
using Apex.Examples.AI.Game;

public class SmartBatController : EntityComponentBase
{
    IMovableActor myMovement_;
    ISensingActor mySenses_;
    IEnemy me_;
    IPhysicsActor myPhysics_;

    public override EntityType type => EntityType.SmartBat;

    private void Awake()
    {
        myMovement_ = GetComponent<IMovableActor>();
        mySenses_ = GetComponent<ISensingActor>();
        mySenses_.LookForPlayerLoS(true, maxDistance: 10);
        me_ = GetComponent<IEnemy>();
        myPhysics_ = GetComponent<IPhysicsActor>();
    }
}
