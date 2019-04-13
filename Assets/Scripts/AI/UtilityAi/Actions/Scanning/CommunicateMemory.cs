#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using Game;

    /// <summary>
    /// This AI action handles communication of memory observations between allied entities within a set range.
    /// </summary>
    public sealed class CommunicateMemory : ActionBase
    {
        [ApexSerialization, FriendlyName("Require Can Communicate", "If set to true, only units whose 'canCommunicate' is true are allowed to communicate their observations")]
        public bool requireCanCommunicate = true;

        [ApexSerialization, FriendlyName("Max Communication Range", "How far away other entities can be before they can no longer receive communicated observations")]
        public float maxCommunicationRange = 25f;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            if (this.requireCanCommunicate && !entity.canCommunicate)
            {
                // entity must be able to communicate, but cannot
                return;
            }

            // get all observations
            var observations = c.memory.allObservations;
            var count = observations.Count;
            if (count == 0)
            {
                // no observations available
                return;
            }

            // iterate through all observations - in order to figure out which allies should receive 'my' observations
            var rangeSqr = this.maxCommunicationRange * this.maxCommunicationRange;
            for (int i = 0; i < count; i++)
            {
                var obs = observations[i];
                var e = obs.entity;

                if (e.type != entity.type)
                {
                    // types do not match - units are not allied
                    continue;
                }

                if ((e.position - entity.position).sqrMagnitude > rangeSqr)
                {
                    // other entity is out of communication range
                    continue;
                }

                var aiEntity = e as IAIEntity;
                if (aiEntity != null)
                {
                    // send all observations to other entity
                    aiEntity.ReceiveCommunicatedMemory(observations);
                }
            }
        }
    }
}