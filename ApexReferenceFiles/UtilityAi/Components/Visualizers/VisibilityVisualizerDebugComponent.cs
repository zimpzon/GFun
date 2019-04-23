#pragma warning disable 1591

namespace Apex.Examples.AI.Components
{
    using Apex.AI;
    using Apex.AI.Visualization;
    using Memory;
    using UnityEngine;

    public class VisibilityVisualizerDebugComponent : ContextGizmoVisualizerComponent
    {
        public EntityType entityType = EntityType.Any;
        public bool showScanRangeSphere;

        [Range(0f, 1f)]
        public float sphereAlphaLevel = 0.2f;

        protected override void DrawGizmos(IAIContext context)
        {
            var c = (AIContext)context;
            var observations = c.memory.allObservations;

            var thisUnit = c.entity;
            var thisPos = this.transform.position;
            var count = observations.Count;
            for (int i = 0; i < count; i++)
            {
                var obs = observations[i];
                if (object.ReferenceEquals(obs.entity, thisUnit))
                {
                    continue;
                }

                var type = obs.entity.type;
                if (this.entityType != EntityType.Any && type != this.entityType)
                {
                    // type does not match
                    continue;
                }

                if (obs.isVisible)
                {
                    // Visible
                    Gizmos.color = Color.green;
                }
                else
                {
                    // Not visible
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawLine(thisPos, obs.position);
            }

            if (this.showScanRangeSphere)
            {
                var color = c.entity.gameObject.GetComponent<Renderer>().material.color;
                Gizmos.color = new Color(color.r, color.g, color.b, this.sphereAlphaLevel);
                Gizmos.DrawSphere(thisUnit.position, thisUnit.scanRange);
            }
        }
    }
}