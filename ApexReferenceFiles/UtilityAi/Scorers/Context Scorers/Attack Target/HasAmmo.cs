#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Helpers;

    /// <summary>
    /// An AI scorer for evaluating whether the entity has a certain amount of ammunition (more than the given threshold). 
    /// </summary>
    public sealed class HasAmmo : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Operator Type", "What type of comparison operator is desired")]
        public RangeOperator operatorType = RangeOperator.GreaterThan;

        [ApexSerialization, FriendlyName("Threshold", "Controls the desired threshold for the compare operation")]
        public int threshold = 0;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // Returns true if entity's current ammo is [operator] compared to the threshold
            if (Utilities.IsOperatorTrue(this.operatorType, entity.currentAmmo, this.threshold))
            {
                return this.score;
            }

            return 0f;
        }
    }
}