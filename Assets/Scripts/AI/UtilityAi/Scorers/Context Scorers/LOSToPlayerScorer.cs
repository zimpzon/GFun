﻿#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    public sealed class LOSToPlayerScorer : UtilityCurveLinearBaseScorer
    {
        [ApexSerialization, FriendlyName("Max age", "Max age in seconds to be considered valid")]
        public float MaxAge = 5;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            var visibility = c.entity.HasLOSToPlayer(MaxAge);
            return this.GetScore(c.entity.CurrentNormalizedHealth);
        }
    }
}
