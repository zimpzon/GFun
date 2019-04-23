#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Game;
    using UnityEngine;

    /// <summary>
    /// An AI option scorer for scoring options of type 'IEntity'. In this case, it scores the distance from the context entity to the option entity
    /// </summary>
    public sealed class NearestAttackTargetScorer : OptionScorerWithScore<IEntity>
    {
        [ApexSerialization, FriendlyName("Multiplier", "A multiplier used to scale the calculated magnitude by.")]
        public float multiplier = 1f;

        public override float Score(IAIContext context, IEntity entity)
        {
            var c = (AIContext)context;

            var distance = (entity.position - c.entity.position).magnitude * this.multiplier;
            return Mathf.Max(0f, (this.score - distance));
        }
    }
}