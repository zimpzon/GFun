#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An AI action which makes the context unit fire at its attack target, and optionally set it to face towards the attack target.
    /// </summary>
    public sealed class FireAtAttackTarget : ActionBase
    {
        [ApexSerialization, FriendlyName("Set Facing", "Whether to also rotate the entity so that it faces towards the attack target")]
        public bool setFacing = true;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            if (entity.attackTarget == null)
            {
                // no valid attack target to attack
                return;
            }

            if (this.setFacing)
            {
                // if setting facing, construct a position to look at and make sure it does not differ in the Y-axis, as otherwise the entity could look upwards or downwards
                var lookAtPos = entity.attackTarget.position;
                lookAtPos.y = entity.position.y;
                entity.gameObject.transform.LookAt(lookAtPos);
            }

            // Issue a 'fire at' command against the entity's current attack target
            entity.FireAt(entity.attackTarget);
        }
    }
}