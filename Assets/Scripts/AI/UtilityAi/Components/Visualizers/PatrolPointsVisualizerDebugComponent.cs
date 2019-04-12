#pragma warning disable 1591

namespace Apex.Examples.AI.Components
{
    using Apex.AI;
    using Apex.AI.Visualization;
    using Memory;
    using UnityEngine;

    public sealed class PatrolPointsVisualizerDebugComponent : ContextGizmoVisualizerComponent
    {
        public Color gizmosColor = Color.yellow;

        [Range(0.5f, 4f)]
        public float sphereSize = 1f;

        protected override void DrawGizmos(IAIContext context)
        {
            var c = (AIContext)context;
            var patrolPoints = c.entity.patrolPoints;
            var count = patrolPoints.Count;
            if (count > 0)
            {
                Gizmos.color = this.gizmosColor;
                for (int i = 0; i < count - 1; i++)
                {
                    Gizmos.DrawSphere(patrolPoints[i], this.sphereSize);
                    Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
                }

                Gizmos.DrawSphere(patrolPoints[count - 1], this.sphereSize);
            }
        }
    }
}