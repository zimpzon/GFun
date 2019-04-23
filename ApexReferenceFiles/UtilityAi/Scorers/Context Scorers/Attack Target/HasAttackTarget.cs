#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An AI scorer for evaluating whether the entity has an attack target or not.
    /// </summary>
    public sealed class HasAttackTarget : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Not", "If set to true, the logic is reversed, e.g. used if the desire is to score when there is no attack target (in this case)")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            // check whether entity has an attack target
            if (c.entity.attackTarget == null)
            {
                return this.not ? this.score : 0f;
            }

            return this.not ? 0f : this.score;
        }
    }
}