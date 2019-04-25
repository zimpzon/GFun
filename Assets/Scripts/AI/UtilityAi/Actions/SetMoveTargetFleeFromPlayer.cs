#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class SetMoveTargetFleeFromPlayer : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var target = c.entity.GetRandomFreePosition();
            c.entity.MoveTo(target);
        }
    }
}
