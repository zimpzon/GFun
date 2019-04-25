#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Memory;

    /// <summary>
    /// This AI action sets the entity's move target reference to null.
    /// </summary>
    public sealed class SetMoveTargetToNearbyCoverPosition : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            // Sets the entity's move target to null
            var c = (AIContext)context;
            c.entity.MoveTo(c.entity.NearbyCoverPosition);
        }
    }
}
