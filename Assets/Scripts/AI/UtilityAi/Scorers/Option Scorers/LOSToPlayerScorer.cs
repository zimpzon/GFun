#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    public sealed class LOSToPlayerScorer : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        [ApexSerialization, FriendlyName("Max age", "Max age in seconds to be considered recent")]
        public float MaxAge = 5;

        public override float Score(IAIContext context, Vector3 pos)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            var visibility = c.entity.HasLOSToPlayer(MaxAge);
            if (visibility)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}
