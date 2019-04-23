#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// An AI option scorer for scoring options of type 'Vector3' (e.g. positions). This scorer scores higher when closer to the contextual entity.
    /// </summary>
    [FriendlyName("AI Example Nearest Position Scorer")]
    public sealed class NearestPositionScorer : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization, FriendlyName("Distance Multiplier", "A multiplier used to scale the calculated magnitude/distance by.")]
        public float distanceMultiplier = 1f;

        public override float Score(IAIContext context, Vector3 pos)
        {
            var c = (AIContext)context;

            // Get the distance from the entity to the supplied position
            var distance = (c.entity.position - pos).magnitude * this.distanceMultiplier;

            // Return no less than 0, in case the distance surpasses the score
            return Mathf.Max(0f, (this.score - distance));
        }
    }
}