#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Helpers;
    using UnityEngine;

    /// <summary>
    /// An AI option scorer for scoring options of type 'Vector3' (e.g. positions). In this case, it scores if there is visibility from the option position to the context entity's current attack target.
    /// </summary>
    public sealed class LOSToAttackTargetScorer : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context, Vector3 pos)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // Get visibility by casting ray against obstacle layers from the context entity's position to the option position, within the entity's scan range
            var visibility = Utilities.IsVisible(entity.position, pos, entity.scanRange);
            if (visibility)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}