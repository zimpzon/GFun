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
    /// This AI action makes the entity move to a randomly calculated walkable position, optionally also sets a move target.
    /// </summary>
    public sealed class MoveToRandomPosition : ActionBase
    {
        [ApexSerialization, FriendlyName("Destination Range", "How far away the random position should be in units/meters")]
        public float destinationRange = 10f;

        [ApexSerialization, FriendlyName("Max Sample Distance", "How far away at maximum from the desired destination is allowed for the navmesh position sampling")]
        public float maxSampleDistance = 2f;

        [ApexSerialization, FriendlyName("Set Move Target", "Set to true to also set move target in addition to issuing a move command")]
        public bool setMoveTarget = true;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // range must not come under 1 unit
            var range = Mathf.Max(1f, this.destinationRange);

            // get a random position
            var randomPos = entity.position + (Random.onUnitSphere * range);
            randomPos.y = entity.position.y;

            // sample the random position using Unity's navmesh
            UnityEngine.AI.NavMeshHit hit;
#if UNITY_5 || UNITY_2017
            int mask = entity.navMeshAgent.areaMask;
#else
            int mask = entity.navMeshAgent.walkableMask;
#endif
            if (!UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, maxSampleDistance, mask))
            {
                // no valid position identified
                return;
            }

            if (this.setMoveTarget)
            {
                // Sets the entity's move target
                entity.moveTarget = hit.position;
            }

            // Issues a move command to the entity
            entity.MoveTo(hit.position);
        }
    }
}