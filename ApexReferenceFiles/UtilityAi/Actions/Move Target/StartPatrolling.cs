#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// This action makes the entity start on its patrol route, as defined by its patrol points. It can optionally set move target and/or issue the actual move command.
    /// </summary>
    public sealed class StartPatrolling : ActionBase
    {
        [ApexSerialization, FriendlyName("Set Move Target", "Set to true to also set move target in addition to issuing a move command")]
        public bool setMoveTarget = true;

        [ApexSerialization, FriendlyName("Issue Move", "Set to true to also issue an actual move command to the entity")]
        public bool issueMove = false;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // make sure the entity has patrol points
            var patrolPoints = entity.patrolPoints;
            var count = patrolPoints.Count;
            if (count == 0)
            {
                return;
            }

            // Start a new patrol
            if (!c.entity.isPatrolling)
            {
                entity.currentPatrolIndex = 0;
                entity.isPatrolling = true;
            }
            else
            {
                // increment the current patrol point index
                entity.currentPatrolIndex += 1;
                if (entity.currentPatrolIndex >= count)
                {
                    entity.currentPatrolIndex = 0;
                }
            }

            var destination = patrolPoints[entity.currentPatrolIndex];
            if (this.setMoveTarget)
            {
                // if setting move target, then do so
                entity.moveTarget = destination;
            }

            if (this.issueMove)
            {
                // if issuing a move order, then do so
                entity.MoveTo(destination);
            }
        }
    }
}