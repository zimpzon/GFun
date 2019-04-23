#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Apex.AI;
    using Apex.AI.Components;
    using Memory;
    using UnityEngine;

    public abstract class EntityComponentBase : MonoBehaviour, IAIEntity, IContextProvider
    {
        private AIContext _context;

        #region IEntity properties

        public abstract EntityType type
        {
            get;
        }

        public Vector3 position => transform.position;

        #endregion IEntity properties

        private void Awake()
        {
            _context = new AIContext(this);
        }

        protected void Start()
        {
            this.name = string.Concat(this.type, " ", this.transform.parent != null ? this.transform.parent.childCount - 1 : 0);

            // Register this game object and entity so that others can identify it as part of the scanning
            EntityManager.instance.Register(this.gameObject, this);
        }

        protected void OnDisable()
        {
            // Unregister this game object and entity so that it will no longer be scanned by other entities
            EntityManager.instance.Unregister(this.gameObject);
        }

        public void ReceiveCommunicatedMemory(IList<Observation> observations)
        {
            var count = observations.Count;
            for (int i = 0; i < count; i++)
            {
                if (object.ReferenceEquals(observations[i].entity, this))
                {
                    // don't store observation of "self"
                    continue;
                }

                // make new observation so that isVisible can be set to false (because this entity is not actaully seeing the other one)
                var newObs = new Observation(observations[i], false);
                _context.memory.AddOrUpdateObservation(newObs, true);
            }
        }

        public IAIContext GetContext(System.Guid id)
        {
            return _context;
        }
    }
}