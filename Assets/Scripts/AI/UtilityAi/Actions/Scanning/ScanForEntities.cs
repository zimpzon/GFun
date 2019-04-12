#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Game;
    using Helpers;
    using Memory;
    using UnityEngine;

    /// <summary>
    /// This AI action handles the scanning of other entities by using Unity's OverlapSphere and adding an observation for each other valid entity, including recording the visibility state.
    /// </summary>
    public sealed class ScanForEntities : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            var entityManager = EntityManager.instance;

            // Use OverlapSphere for getting all relevant colliders within scan range, filtered by the scanning layer
            var hits = Physics.OverlapSphere(entity.position, entity.scanRange, LayersManagerComponent.instance.unitsLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];

                // Get the IEntity from the hit game object
                var hitEntity = entityManager.GetEntity(hit.gameObject);
                if (hitEntity == null)
                {
                    // entity is not scannable (it has not been registered)
                    continue;
                }

                if (object.ReferenceEquals(hitEntity, entity))
                {
                    // do not store observation of "self"
                    continue;
                }

                if (hitEntity.isDead)
                {
                    // ignore dead entities (they may not have been cleaned up yet)
                    continue;
                }

                // Get visibility by casting ray against obstacle layers
                var visibility = Utilities.IsVisible(entity.position, hitEntity.position, entity.scanRange);

                // create and add (or update) the observation
                var observation = new Observation(hitEntity, visibility);
                c.memory.AddOrUpdateObservation(observation);
            }
        }
    }
}