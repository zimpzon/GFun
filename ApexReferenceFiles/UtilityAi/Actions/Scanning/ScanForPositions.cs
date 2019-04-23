#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;
    using UnityEngine;
#if UNITY_2017 || UNITY_5_5 || UNITY_5_6
    using UnityEngine.AI;
#endif

    /// <summary>
    /// This AI action handles the scanning and storing of valid, walkable positions in a square around the entity.
    /// </summary>
    public sealed class ScanForPositions : ActionBase
    {
        [ApexSerialization, FriendlyName("Sampling Range", "How large a range points are sampled in, in a square with the entity in the center")]
        public float samplingRange = 12f;

        [ApexSerialization, FriendlyName("Sampling Density", "How much distance there is between individual point samples")]
        public float samplingDensity = 1.5f;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // clear any previously sampled positions
            c.sampledPositions.Clear();

            var halfSamplingRange = this.samplingRange * 0.5f;
            var pos = entity.position;

            // nested loop in x and z directions, starting at negative half sampling range and ending at positive half sampling range, thus sampling in a square around the entity
            for (var x = -halfSamplingRange; x < halfSamplingRange; x += this.samplingDensity)
            {
                for (var z = -halfSamplingRange; z < halfSamplingRange; z += this.samplingDensity)
                {
                    var p = new Vector3(pos.x + x, 0f, pos.z + z);

                    // Sample the position in the navigation mesh to ensure that the desired position is actually walkable
                    UnityEngine.AI.NavMeshHit hit;
#if UNITY_5 || UNITY_2017
                    int mask = entity.navMeshAgent.areaMask;
#else
                    int mask = entity.navMeshAgent.walkableMask;
#endif
                    if (UnityEngine.AI.NavMesh.SamplePosition(p, out hit, this.samplingDensity * 0.5f, mask))
                    {
                        // only walkable positions are added to the list
                        c.sampledPositions.Add(hit.position);
                    }
                }
            }
        }
    }
}