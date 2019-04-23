#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Game;

    /// <summary>
    /// An AI option scorer for scoring options of type 'IEntity'. Thus, this scorer evaluates the current health
    /// </summary>
    public sealed class AttackTargetCurrentHealthScorer : OptionScorerWithScore<IEntity>
    {
        [ApexSerialization, FriendlyName("Multiplier", "A multiplier used to scale the IEntity's current health")]
        public float multiplier = 1f;

        [ApexSerialization]
        public bool reversed = false;

        public override float Score(IAIContext context, IEntity entity)
        {
            // Get the entity's current health scaled with the desired multiplier
            var val = entity.currentHealth * this.multiplier;
            return this.reversed ? -val : val;
        }
    }
}