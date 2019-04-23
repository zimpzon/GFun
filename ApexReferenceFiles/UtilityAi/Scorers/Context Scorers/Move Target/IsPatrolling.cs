#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An AI scorer for evaluating whether the entity is currently patrolling or not.
    /// </summary>
    public sealed class IsPatrolling : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            // check whether the entity is patrolling
            if (c.entity.isPatrolling)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}