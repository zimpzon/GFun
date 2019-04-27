#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class MoveToRandomPosition : ActionBase, IRequireTermination
    {
        public override void Execute(IAIContext context)
            => ((AIContext)context).entity.MoveToRandomNearbyPosition();

        public void Terminate(IAIContext context)
            => ((AIContext)context).entity.MoveToRandomNearbyPosition_Terminate();
    }
}
