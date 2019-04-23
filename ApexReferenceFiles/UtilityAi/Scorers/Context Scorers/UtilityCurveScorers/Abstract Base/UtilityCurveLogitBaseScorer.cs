#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.Serialization;
    using Helpers;

    /// <summary>
    /// A base class to use for a specific utility curve scorer, utilizing a logit (inverse of sigmoidal 'logistic' function) curve.
    /// </summary>
    public abstract class UtilityCurveLogitBaseScorer : UtilityCurveSpecificBaseScorer
    {
        [ApexSerialization]
        public float logit = 1f;

        [ApexSerialization]
        public float max = 10f;

        public override float GetScore(float input)
        {
            return this.reversed ?
                Utilities.GetReverseLogit(input, this.logit, this.max) * this.scoreMultiplier :
                Utilities.GetLogit(input, this.logit, this.max) * this.scoreMultiplier;
        }
    }
}