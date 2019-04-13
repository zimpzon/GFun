#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    /// <summary>
    /// This AI action sets the entity's attack target to null
    /// </summary>
    public sealed class SetAttackTargetToNull : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            // sets the entity's attack target to null
            var c = (AIContext)context;
            c.entity.attackTarget = null;
        }
    }
}