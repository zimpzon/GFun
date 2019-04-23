#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using UnityEngine;

    public class EntityManager
    {
        public static readonly EntityManager instance = new EntityManager();

        private Dictionary<GameObject, IEntity> _entities;

        public EntityManager()
        {
            _entities = new Dictionary<GameObject, IEntity>(5);
        }

        public void Register(GameObject go, IEntity entity)
        {
            _entities.Add(go, entity);
        }

        public IEntity GetEntity(GameObject go)
        {
            IEntity entity = null;
            _entities.TryGetValue(go, out entity);
            return entity;
        }

        public void Unregister(GameObject go)
        {
            _entities.Remove(go);
        }
    }
}