#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    public sealed class HasNearbyCoverScorer : UtilityCurveLinearBaseScorer
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            bool hasCover = c.entity.HasNearbyCover;
            return this.GetScore(hasCover ? 100 : 0);
        }
    }
}
