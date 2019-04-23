#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Helpers;

    /// <summary>
    /// A base class to use for a specific utility curve scorer, utilizing an gaussian curve.
    /// </summary>
    public abstract class UtilityCurveGaussianBaseScorer : UtilityCurveSpecificBaseScorer
    {
        [ApexSerialization, FriendlyName("Curve Height", "The maximum height of the gaussian curve (the amplitude)")]
        public float curveHeight = 1f;

        [ApexSerialization, FriendlyName("Curve Center", "The center point for the gaussian curve")]
        public float curveCenter = 0.5f;

        [ApexSerialization, FriendlyName("Curve Width", "The width of the gaussian curve.")]
        public float curveWidth = 0.2f;

        /// <summary>
        /// Gets the utility curve score. Should always return values in the range of 0-1, although this can be overriden by adjusting the parameters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The utility score
        /// </returns>
        public override float GetScore(float input)
        {
            return this.reversed ?
                Utilities.GetReversedGaussian(input, this.curveHeight, this.curveCenter, this.curveWidth) * this.scoreMultiplier :
                Utilities.GetGaussian(input, this.curveHeight, this.curveCenter, this.curveWidth) * this.scoreMultiplier;
        }
    }
}