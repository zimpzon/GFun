#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Helpers;

    /// <summary>
    /// A base class to use for a specific utility curve scorer, utilizing a linear curve.
    /// </summary>
    public abstract class UtilityCurveLinearBaseScorer : UtilityCurveSpecificBaseScorer
    {
        [ApexSerialization, FriendlyName("Linear Slope", "The slope (a) for the linear curve. Positive means increasing, negative means decreasing.")]
        public float linearSlope = 1f;

        [ApexSerialization, FriendlyName("Max", "The maximum expected value for the input.")]
        public float max = 100f;

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
                Utilities.GetReverseLinear(input, this.linearSlope, this.max) * this.scoreMultiplier :
                Utilities.GetLinear(input, this.linearSlope, this.max) * this.scoreMultiplier;
        }
    }
}