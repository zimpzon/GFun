#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class MoveToMoveTarget : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            if (entity.MoveTarget == null)
                return;

            entity.MoveTo(entity.MoveTarget);
        }
    }
}