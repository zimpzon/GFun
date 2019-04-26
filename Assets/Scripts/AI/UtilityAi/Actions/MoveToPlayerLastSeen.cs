#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class SetMoveTargetToSeenPlayerPosition : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            c.entity.MoveToPlayerLatestSeenPosition();
        }
    }
}