#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class FleeFromPlayer : ActionBase, IRequireTermination
    {
        public override void Execute(IAIContext context)
            => ((AIContext)context).entity.FleeFromPlayer();

        public void Terminate(IAIContext context)
            => ((AIContext)context).entity.FleeFromPlayer_Terminate();
    }
}