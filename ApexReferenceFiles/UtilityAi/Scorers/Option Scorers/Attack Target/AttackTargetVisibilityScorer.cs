#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Game;
    using Helpers;

    /// <summary>
    /// An AI option scorer for scoring options of type 'IEntity'. In this case, it scores if the option entity is visible from the context entity, within its scan range.
    /// </summary>
    public sealed class AttackTargetVisibilityScorer : OptionScorerWithScore<IEntity>
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context, IEntity attackTarget)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // get the visibility from the context entity to the option entity within the context entity's scan range
            var visibility = Utilities.IsVisible(entity.position, attackTarget.position, entity.scanRange);
            if (visibility)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}