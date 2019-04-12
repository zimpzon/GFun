#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using System.Collections.Generic;
    using Apex.AI;
    using Game;
    using Memory;
    using UnityEngine;

    /// <summary>
    /// The AI context object used in the Apex AI Examples project
    /// </summary>
    public class AIContext : IAIContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIContext"/> class.
        /// </summary>
        /// <param name="entity">The entity for whom this context belongs to.</param>
        public AIContext(IAIEntity entity)
        {
            this.entity = entity;
            this.sampledPositions = new List<Vector3>(64);
            this.memory = new AIMemory();
        }

        /// <summary>
        /// Gets the entity owning this context object.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IAIEntity entity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the entity's memory.
        /// </summary>
        /// <value>
        /// The memory.
        /// </value>
        public AIMemory memory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the sampled positions. They will also be valid, walkable potential destinations.
        /// </summary>
        /// <value>
        /// The sampled positions.
        /// </value>
        public List<Vector3> sampledPositions
        {
            get;
            private set;
        }
    }
}