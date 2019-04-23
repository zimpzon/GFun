#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// A specific utility curve scorer for scoring the entity's current ammunition count.
    /// </summary>
    public sealed class UtilityCurveAmmoScorer : UtilityCurveExponentialBaseScorer
    {
        [ApexSerialization, FriendlyName("Ammo Multiplier", "A factor used to multiply the entity's current ammunition count before getting the corresponding utility score.")]
        public float ammoMultiplier = 10f;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            return this.GetScore(c.entity.currentAmmo * this.ammoMultiplier);
        }
    }
}