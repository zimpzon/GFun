#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    public sealed class TimeSinceLatestMoveScorer : UtilityCurveLinearBaseScorer
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            return this.GetScore(c.entity.TimeSinceLatestMoveCommand);
        }
    }
}
