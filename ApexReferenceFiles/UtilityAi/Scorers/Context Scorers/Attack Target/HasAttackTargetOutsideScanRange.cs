namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class HasAttackTargetOutsideScanRange : ContextualScorerBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // Get the entity's move target
            var attackTarget = entity.attackTarget;
            if (attackTarget == null)
            {
                return 0f;
            }

            // get the squared distance from the move target to the entity and compare this with the desired range
            var distance = (entity.position - attackTarget.position).sqrMagnitude;
            if (distance > (entity.scanRange * entity.scanRange))
            {
                return this.score;
            }

            return 0f;
        }
    }
}