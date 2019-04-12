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
    /// An AI option scorer for scoring options of type 'Vector3' (positions). 
    /// </summary>
    public sealed class NextToBlockScorer : OptionScorerWithScore<Vector3>
    {
        [ApexSerialization]
        public float samplingDistance = 1f;

        public override float Score(IAIContext context, Vector3 pos)
        {
            var c = (AIContext)context;

            UnityEngine.AI.NavMeshHit hit;
#if UNITY_5 || UNITY_2017
            var layer = c.entity.navMeshAgent.areaMask;
#else
            var layer = c.entity.navMeshAgent.walkableMask;
#endif

            // Sample in 4 directions (not diagonal) and use the NavMesh for sampling the position with a very small threshold (Mathf.Epsilon)
            var p1 = new Vector3(pos.x - this.samplingDistance, 0f, pos.z);
            if (!UnityEngine.AI.NavMesh.SamplePosition(p1, out hit, Mathf.Epsilon, layer))
            {
                return this.score;
            }

            var p2 = new Vector3(pos.x + this.samplingDistance, 0f, pos.z);
            if (!UnityEngine.AI.NavMesh.SamplePosition(p2, out hit, Mathf.Epsilon, layer))
            {
                return this.score;
            }

            var p3 = new Vector3(pos.x, 0f, pos.z - this.samplingDistance);
            if (!UnityEngine.AI.NavMesh.SamplePosition(p3, out hit, Mathf.Epsilon, layer))
            {
                return this.score;
            }

            var p4 = new Vector3(pos.x, 0f, pos.z + this.samplingDistance);
            if (!UnityEngine.AI.NavMesh.SamplePosition(p4, out hit, Mathf.Epsilon, layer))
            {
                return this.score;
            }

            return 0f;
        }
    }
}