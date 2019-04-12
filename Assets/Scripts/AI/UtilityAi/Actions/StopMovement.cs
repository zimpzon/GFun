#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    /// <summary>
    /// This AI action makes the entity stop its movement immediately, by issuing 'Stop' to teh nav mesh agent.
    /// </summary>
    public sealed class StopMovement : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            // Stops all nav mesh agent movement
            var c = (AIContext)context;
#if UNITY_2017
            c.entity.navMeshAgent.isStopped = true;
#else
            c.entity.navMeshAgent.Stop();
#endif
        }
    }
}