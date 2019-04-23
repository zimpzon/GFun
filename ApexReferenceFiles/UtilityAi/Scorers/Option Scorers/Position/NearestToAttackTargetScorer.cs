#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// An AI option scorer for scoring options of type 'Vector3'. This scorer scores higher if the option position is nearer to the contextual entity's current attack target.
    /// </summary>
    public sealed class NearestToAttackTargetScorer : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization, FriendlyName("Distance Multiplier", "A multiplier used to scale the calculated magnitude/distance by.")]
        public float distanceMultiplier = 1f;

        public override float Score(IAIContext context, Vector3 pos)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // ensure there is a valid attack target
            var attackTarget = entity.attackTarget;
            if (attackTarget == null)
            {
                return 0f;
            }

            // calculate the distance from attack target to option position, optionally scale and ensure that the final output score is never less than 0 (if distance surpasses score)
            var distance = (attackTarget.position - pos).magnitude * this.distanceMultiplier;
            return Mathf.Max(0f, (this.score - distance));
        }
    }
}