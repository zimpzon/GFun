#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// This AI action makes the entity stop its patrolling, either permanently or as a 'pause' of the patrolling.
    /// </summary>
    public sealed class StopPatrolling : ActionBase
    {
        [ApexSerialization, FriendlyName("Pause Patrol", "Set to true to not stop the patrolling, but just pause it, so that it can continue from where it came.")]
        public bool pausePatrol = true;

        public override void Execute(IAIContext context)
        {
            // set the entity to not be patrolling right now
            var c = (AIContext)context;
            c.entity.isPatrolling = false;

            if (!this.pausePatrol)
            {
                // if not pausing - then reset the current patrol index (the next patrol point used as move target)
                c.entity.currentPatrolIndex = 0;
            }
        }
    }
}