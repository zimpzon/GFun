#pragma warning disable 1591

namespace Apex.Examples.AI.Components
{
    using Apex.AI;
    using Apex.AI.Visualization;
    using Memory;
    using UnityEngine;

    public class AttackTargetVisualizerDebugComponent : ContextGizmoVisualizerComponent
    {
        public Color gizmosColor = Color.red;

        [Range(0.5f, 4f)]
        public float sphereSize = 2f;

        protected override void DrawGizmos(IAIContext context)
        {
            var c = (AIContext)context;
            var attackTarget = c.entity.attackTarget;
            if (attackTarget != null)
            {
                Gizmos.color = this.gizmosColor;
                Gizmos.DrawWireSphere(attackTarget.position, this.sphereSize);
            }
        }
    }
}