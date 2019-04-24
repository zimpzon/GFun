#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class SetMoveTargetToPlayerPosition : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            c.entity.MoveTo(c.entity.PlayerPosition);
        }
    }
}