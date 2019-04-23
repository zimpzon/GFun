#pragma warning disable 1591

namespace Apex.Examples.AI.Components
{
    using Apex.AI;
    using Apex.AI.Visualization;
    using Memory;
    using UnityEngine;

    public class MoveTargetVisualizerDebugComponent : ContextGizmoVisualizerComponent
    {
        public Color gizmosColor = Color.yellow;

        [Range(0.5f, 4f)]
        public float sphereSize = 2f;

        protected override void DrawGizmos(IAIContext context)
        {
            var c = (AIContext)context;
            var moveTarget = c.entity.moveTarget;
            if (!moveTarget.HasValue)
            {
                return;
            }

            Gizmos.color = this.gizmosColor;
            Gizmos.DrawWireSphere(moveTarget.Value, this.sphereSize);
        }
    }
}