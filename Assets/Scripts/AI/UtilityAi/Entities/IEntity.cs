#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using UnityEngine;

    public interface IEntity
    {
        /// <summary>
        /// Gets the type of entity.
        /// </summary>
        /// <value>
        /// The entity type.
        /// </value>
        EntityType type { get; }

        /// <summary>
        /// Gets the Unity game object.
        /// </summary>
        /// <value>
        /// The game object.
        /// </value>
        GameObject gameObject { get; }

        /// <summary>
        /// Wrapper for getting the position easily.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        Vector3 position { get; }

        /// <summary>
        /// Gets the attack range.
        /// </summary>
        /// <value>
        /// The attack range.
        /// </value>
        float attackRange { get; }

        /// <summary>
        /// Gets the minimum damage.
        /// </summary>
        /// <value>
        /// The minimum damage.
        /// </value>
        float minDamage { get; }

        /// <summary>
        /// Gets the maximum damage.
        /// </summary>
        /// <value>
        /// The maximum damage.
        /// </value>
        float maxDamage { get; }

        /// <summary>
        /// Gets the maximum health.
        /// </summary>
        /// <value>
        /// The maximum health.
        /// </value>
        float maxHealth { get; }

        /// <summary>
        /// Gets or sets the current health.
        /// </summary>
        /// <value>
        /// The current health.
        /// </value>
        float currentHealth { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is dead.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dead; otherwise, <c>false</c>.
        /// </value>
        bool isDead { get; }

        /// <summary>
        /// Fires at another entity, damaging the other entity a random amount between min and max damage.
        /// </summary>
        /// <param name="target">The target.</param>
        void FireAt(IEntity target);
    }
}