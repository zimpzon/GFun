#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    /// <summary>
    /// This AI action makes the entity move towards its current move target, if it has a valid value.
    /// </summary>
    public sealed class MoveToMoveTarget : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            if (!entity.moveTarget.HasValue)
            {
                // no valid move target to move to
                return;
            }

            // Issue the move order
            entity.MoveTo(entity.moveTarget.Value);
        }
    }
}