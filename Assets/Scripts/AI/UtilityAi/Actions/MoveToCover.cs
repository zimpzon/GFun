﻿#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class MoveToCover : ActionBase
    {
        public override void Execute(IAIContext context)
            => ((AIContext)context).entity.MoveToCover();

        public void Terminate(IAIContext context)
            => ((AIContext)context).entity.MoveToCover_Terminate();
    }
}
