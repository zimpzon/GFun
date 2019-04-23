#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// An AI option scorer for scoring options of type 'Vector3' (e.g. positions). This option scorer scores positions that are closer to desired range to the nearest ally (nearest ally is calculated for each position). 
    /// </summary>
    public sealed class ProximityToNearestAlly : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization, FriendlyName("Desired Range", "The desired range to score highest at, i.e. at entities at this range from the option position results in the highest scores.")]
        public float desiredRange = 4f;

        public override float Score(IAIContext context, Vector3 position)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // get all observations and ensure that there are some
            var observations = c.memory.allObservations;
            var count = observations.Count;
            if (count == 0)
            {
                return 0f;
            }

            // iterate through all observations, ignoring non-allies, and figure out which allied observation is the nearest one
            var nearest = Vector3.zero;
            var shortest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var obs = observations[i];
                if (obs.entity.type != entity.type)
                {
                    // observation is not an allied unit (of same type as self)
                    continue;
                }

                var distance = (entity.position - obs.position).sqrMagnitude;
                if (distance < shortest)
                {
                    shortest = distance;
                    nearest = obs.position;
                }
            }

            if (nearest.sqrMagnitude == 0f)
            {
                return 0f;
            }

            // calculate the final output score: the highest scores are achieved when the calculated range is exactly the same as the desired range, and the output score can never be less than 0
            var range = (position - nearest).magnitude;
            return Mathf.Max(0f, (this.score - Mathf.Abs(this.desiredRange - range)));
        }
    }
}