#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// An AI option scorer for scoring options of type 'Vector3'. This option scorer scores positions highest if they are at the desired range, where the nearest enemy is identified for each position.
    /// </summary>
    public sealed class ProximityToNearestEnemy : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization, FriendlyName("Desired Range", "The desired range at which entities score the highest.")]
        public float desiredRange = 14f;

        public override float Score(IAIContext context, Vector3 position)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // get all observations and ensure that there are any
            var observations = c.memory.allObservations;
            var count = observations.Count;
            if (count == 0)
            {
                return 0f;
            }

            // identify the nearest enemy by iterating through all observations
            var nearest = Vector3.zero;
            var shortest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var obs = observations[i];
                if (obs.entity.type == entity.type)
                {
                    // observation is not an enemy unit (of different type as self)
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