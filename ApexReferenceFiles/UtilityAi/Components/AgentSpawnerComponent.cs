#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System;
    using UnityEngine;

    public sealed class AgentSpawnerComponent : MonoBehaviour
    {
        public Transform initialMoveTarget;
        public Transform[] spawnPositions;
        public GameObject prefab;
        public Rect buttonRect;

        public int bulkSpawnCount = 10;

        private Rect _altRect;

        private int _currentSpawnIndex = 0;

        private void Awake()
        {
            if (this.spawnPositions.Length == 0)
            {
                throw new ArgumentNullException("spawnPosition", "No valid spawn positions provided for " + this.ToString());
            }

            if (this.prefab == null)
            {
                throw new ArgumentNullException("dronePrefab", "No valid drone prefab supplied for " + this.ToString());
            }

            _altRect = new Rect(Screen.width - buttonRect.x - buttonRect.width, buttonRect.y, buttonRect.width, buttonRect.height);
        }

        public void OnGUI()
        {
            if (this.prefab == null || this.spawnPositions.Length == 0)
            {
                return;
            }

            if (GUI.Button(this.buttonRect, string.Concat("Spawn ", this.prefab.name)))
            {
                SpawnNewDrone();
            }

            if (GUI.Button(_altRect, string.Concat("Bulk Spawn ", this.prefab.name)))
            {
                for (int i = 0; i < this.bulkSpawnCount; i++)
                {
                    SpawnNewDrone();
                }
            }
        }

        private void SpawnNewDrone()
        {
            var newGO = Instantiate(this.prefab, this.spawnPositions[_currentSpawnIndex].position, Quaternion.identity) as GameObject;
            newGO.transform.SetParent(this.transform);

            if (this.initialMoveTarget != null)
            {
                var entity = newGO.GetComponent<EntityComponentBase>();
                entity.moveTarget = this.initialMoveTarget.position;
            }

            if (++_currentSpawnIndex >= this.spawnPositions.Length)
            {
                _currentSpawnIndex = 0;
            }
        }
    }
}