#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// This AI action makes sure that there are no null or dead entity entries in the memory. Additionally, it sets 'expired' observations to be not visible.
    /// </summary>
    public sealed class CleanupMemory : ActionBase
    {
        [ApexSerialization, FriendlyName("Visibility Expiration Threshold", "How many seconds old an observation is before its 'isVisible' is set to false")]
        public float visibilityExpirationThreshold = 1.5f;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var memory = c.memory;

            // get all observations from memory
            var observations = memory.allObservations;
            var count = observations.Count;
            if (count == 0)
            {
                // there are no available observations
                return;
            }

            // iterate through all observations
            for (int i = count - 1; i >= 0; i--)
            {
                var obs = observations[i];
                if (obs.entity == null || obs.entity.isDead)
                {
                    // remove dead or invalid entity observations
                    memory.RemoveObservationAt(i);
                }
                else if ((Time.time - obs.timestamp) >= this.visibilityExpirationThreshold)
                {
                    // the visibility setting on this observation has expired - since it was not updated recently, it must no longer be visible
                    obs.isVisible = false;
                }
            }
        }
    }
}