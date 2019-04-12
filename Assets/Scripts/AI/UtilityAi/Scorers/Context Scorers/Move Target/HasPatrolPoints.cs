#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    // An AI scorer for evaluating whether the entity has any patrol points or not.
    public sealed class HasPatrolPoints : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            // check whether the entity has a valid patrol points array with more any elements
            var patrolPoints = c.entity.patrolPoints;
            if (patrolPoints != null && patrolPoints.Count > 0)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}