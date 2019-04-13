#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Helpers;

    /// <summary>
    /// An AI scorer for evaluating the range to the entity's current move target.
    /// </summary>
    public sealed class CheckMoveTargetRange : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Operator Type", "What type of comparison operator is desired")]
        public RangeOperator operatorType = RangeOperator.LessThanOrEquals;

        [ApexSerialization, FriendlyName("Range", "What range to use in the comparison operator")]
        public float range = 2f;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // Get the entity's move target
            var moveTarget = entity.moveTarget;
            if (!moveTarget.HasValue)
            {
                return 0f;
            }

            // get the squared distance from the move target to the entity and compare this with the desired range
            var distance = (entity.position - moveTarget.Value).sqrMagnitude;
            if (Utilities.IsOperatorTrue(this.operatorType, distance, this.range * this.range))
            {
                return this.score;
            }

            return 0f;
        }
    }
}