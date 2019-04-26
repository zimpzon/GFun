#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    public sealed class MoveTargetReachedScorer : UtilityCurveLinearBaseScorer
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            bool moveDone = c.entity.MoveTargetReached;
            return this.GetScore(moveDone ? 100 : 0);
        }
    }
}
