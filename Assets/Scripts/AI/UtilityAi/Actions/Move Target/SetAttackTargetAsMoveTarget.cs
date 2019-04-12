#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// This AI action sets the entity's current attack target as the move target, effectively creating a new move target where the attack target is currently.
    /// </summary>
    public sealed class SetAttackTargetAsMoveTarget : ActionBase
    {
        [ApexSerialization, FriendlyName("Overwrite Move Target", "Whether to overwrite existing move target if it exists (TRUE)")]
        public bool overwriteMoveTarget = true;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            if (entity.attackTarget == null)
            {
                // if there is no valid attack target, then the attack target cannot be used for move target
                return;
            }

            if (!this.overwriteMoveTarget && entity.moveTarget.HasValue)
            {
                // if not supposed to overwrite move targets, but we already have a move target, then stop here
                return;
            }

            // we find the nearest sampled position, because we know the sampled positions are valid (on the NavMesh) - so this is to prevent invalid moves
            var pos = entity.attackTarget.position;
            var nearest = Vector3.zero;
            var shortest = float.MaxValue;

            var count = c.sampledPositions.Count;
            for (int i = 0; i < count; i++)
            {
                var distance = (entity.position - pos).sqrMagnitude;
                if (distance < shortest)
                {
                    shortest = distance;
                    nearest = pos;
                }
            }

            // set the move target value
            entity.moveTarget = nearest;
        }
    }
}