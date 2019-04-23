#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Apex.AI;
    using Apex.AI.Components;
    using Memory;
    using UnityEngine;

    [RequireComponent(typeof(EnemyScript))]
    public abstract class EntityComponentBase : MonoBehaviour, IAIEntity, IContextProvider
    {
        AIContext context_;
        EnemyScript me_;
        ISensingActor mySenses_;

        #region IEntity properties

        public abstract EntityType AiType
        {
            get;
        }

        public float CurrentNormalizedHealth => (me_.Life / me_.MaxLife) * 100;
        public bool HasLOSToPlayer(float maxAge) => mySenses_.GetPlayerLatestKnownPositionAge() < maxAge;
        public Vector3 Position => me_.GetPosition();
        public Vector3 PlayerLatestSeenPosition => mySenses_.GetPlayerLatestKnownPosition(PlayerPositionType.Tile);
        public Vector3 PlayerPosition => AiBlackboard.Instance.PlayerPosition;

        #endregion IEntity properties

        public void Awake()
        {
            context_ = new AIContext(this);
            me_ = GetComponent<EnemyScript>();
            mySenses_ = GetComponent<ISensingActor>();
        }

        protected void Start()
        {
            this.name = string.Concat(this.AiType, " ", this.transform.parent != null ? this.transform.parent.childCount - 1 : 0);

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
                context_.memory.AddOrUpdateObservation(newObs, true);
            }
        }

        public IAIContext GetContext(System.Guid id)
        {
            if (context_ == null)
                throw new System.InvalidOperationException("Did you forget to call base.Awake in your AI controller?");

            return context_;
        }
    }
}