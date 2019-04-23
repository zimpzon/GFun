#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An AI scorer for evaluating whether the entity has velocity currently, or not.
    /// </summary>
    public sealed class HasVelocity : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Use Desired Velocity", "There are 2 types of velocity: Actual velocity and desired velocity. The actual velocity is the desired velocity + any undesired forces (e.g. gravity, force push, etc.).")]
        public bool useDesiredVelocity = false;

        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // check whether the chosen velocity has any magnitude or not
            var velocity = this.useDesiredVelocity ? entity.navMeshAgent.desiredVelocity : entity.navMeshAgent.velocity;
            if (velocity.sqrMagnitude > 0f)
            {
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}