#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Helpers;

    /// <summary>
    /// An AI scorer for evaluating whether the entity's attack target is visible or not, within the given range.
    /// </summary>
    public sealed class IsAttackTargetVisible : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        [ApexSerialization, FriendlyName("Use Scan Range", "Set to true to use the scan range"), MemberDependency("useAttackRange", false)]
        public bool useScanRange = false;

        [ApexSerialization, FriendlyName("Use Attack Range", "Set to true to use the attack range"), MemberDependency("useScanRange", false)]
        public bool useAttackRange = false;

        [ApexSerialization, FriendlyName("Custom Range", "Input a custom range here (if not using scan or attack range"), MemberDependency("useScanRange", false), MemberDependency("useAttackRange", false)]
        public float customRange = 10f;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // get the entity's attack target
            var attackTarget = entity.attackTarget;
            if (attackTarget == null)
            {
                return 0f;
            }

            // prepare the range to use for visibility check
            var range = this.customRange;
            if (this.useScanRange)
            {
                range = entity.scanRange;
            }
            else if (this.useAttackRange)
            {
                range = entity.attackRange;
            }

            // Get the visibility from the entity to the entity's attack target
            var visibility = Utilities.IsVisible(entity.position, entity.attackTarget.position, range);
            if (visibility)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}