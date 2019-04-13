#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An AI scorer for evaluating whether the entity has a move target currently.
    /// </summary>
    public sealed class HasMoveTarget : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            // check whether the entity has a move target
            if (!c.entity.moveTarget.HasValue)
            {
                return this.not ? this.score : 0f;
            }

            return this.not ? 0f : this.score;
        }
    }
}