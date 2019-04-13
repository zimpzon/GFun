#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    /// <summary>
    /// This AI action makes the entity reload
    /// </summary>
    public sealed class ReloadAction : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            // make the entity reload
            var c = (AIContext)context;
            c.entity.Reload();
        }
    }
}