#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Memory;
    using UnityEngine;
#if UNITY_2017 || UNITY_5_5 || UNITY_5_6
    using UnityEngine.AI;
#endif

    public interface IAIEntity : IEntity
    {
        /// <summary>
        /// Gets the Unity NavMeshAgent.
        /// </summary>
        /// <value>
        /// The navigation mesh agent.
        /// </value>
        UnityEngine.AI.NavMeshAgent navMeshAgent { get; }

        /// <summary>
        /// Gets or sets the current move target.
        /// </summary>
        /// <value>
        /// The move target.
        /// </value>
        Vector3? moveTarget { get; set; }

        /// <summary>
        /// Gets or sets the current attack target.
        /// </summary>
        /// <value>
        /// The attack target.
        /// </value>
        IEntity attackTarget { get; set; }

        /// <summary>
        /// Gets the current ammunition count.
        /// </summary>
        /// <value>
        /// The current ammunition count.
        /// </value>
        int currentAmmo { get; }

        /// <summary>
        /// Gets the maximum ammunition count.
        /// </summary>
        /// <value>
        /// The maximum ammunition count.
        /// </value>
        int maxAmmo { get; }

        /// <summary>
        /// Gets the scan range.
        /// </summary>
        /// <value>
        /// The scan range.
        /// </value>
        float scanRange { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can communicate.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can communicate; otherwise, <c>false</c>.
        /// </value>
        bool canCommunicate { get; }

        /// <summary>
        /// Gets the patrol points.
        /// </summary>
        /// <value>
        /// The patrol points.
        /// </value>
        IList<Vector3> patrolPoints { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is patrolling.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is patrolling; otherwise, <c>false</c>.
        /// </value>
        bool isPatrolling { get; set; }

        /// <summary>
        /// Gets the index of the current patrol destination.
        /// </summary>
        /// <value>
        /// The index of the current patrol destination.
        /// </value>
        int currentPatrolIndex { get; set; }

        /// <summary>
        /// Orders this entity to move to the supplied destination by utilizing the NavMeshAgent.
        /// </summary>
        /// <param name="destination">The destination.</param>
        void MoveTo(Vector3 destination);

        /// <summary>
        /// Reloads this entities weapon, meaning that current ammo it set to maximum ammo.
        /// </summary>
        void Reload();

        /// <summary>
        /// Receives a list of communicated memory observations and adds newer observations to own memory.
        /// </summary>
        /// <param name="observations">The observations.</param>
        void ReceiveCommunicatedMemory(IList<Observation> observations);
    }
}