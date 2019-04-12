#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An AI scorer for evaluating whether the entity's current attack target is dead or not.
    /// </summary>
    public sealed class IsAttackTargetDead : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // check whether the entity has an attack target
            if (entity.attackTarget == null)
            {
                return 0f;
            }

            // check whether the entity's attack target is dead
            if (entity.attackTarget.isDead)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}