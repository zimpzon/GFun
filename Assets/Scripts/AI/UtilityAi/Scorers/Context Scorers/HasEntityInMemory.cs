#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// This AI scorer can be used in a range of situations. It can evaluate whether a given entity has any observations in memory fulfilling a range of variable 'filters', e.g. visibility, entity type or whether they are allied or not.
    /// </summary>
    public sealed class HasEntityInMemory : ContextualScorerBase
    {
        [ApexSerialization, FriendlyName("Entity Type", "Only entities in memory of this desired type are considered, unless set to 'Any'")]
        public EntityType entityType = EntityType.Any;

        [ApexSerialization, FriendlyName("Custom Range", "The custom range to consider other entities at, set to '0' to disable completely"), MemberDependency("useScanRange", false), MemberDependency("useAttackRange", false)]
        public float customRange = 0f;

        [ApexSerialization, FriendlyName("Use Scan Range", "Whether to use the entity's scanning range as the maximum allowed range for enmies to factor in to the count"), MemberDependency("useAttackRange", false)]
        public bool useScanRange = true;

        [ApexSerialization, FriendlyName("Use Attack Range", "Whether to use the entity's attack range as the maximum allowed range for enemies to factor in to the count"), MemberDependency("useScanRange", false)]
        public bool useAttackRange = false;

        [ApexSerialization, FriendlyName("Only Visible", "Whether to filter out any currently non-visible entities in memory")]
        public bool onlyVisible = false;

        [ApexSerialization, FriendlyName("Skip Allies", "Whether to skip other entities of the same type as this entity (which are considered allies)")]
        public bool skipAllies = false;

        [ApexSerialization, FriendlyName("Not", "Set to true to reverse the logic of the scorer")]
        public bool not = false;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // get all observations from memory and ensure there are some before going on
            var observations = c.memory.allObservations;
            var count = observations.Count;
            if (count == 0)
            {
                return 0f;
            }

            // figure out what to use for range
            var rangeSqr = 0f;
            if (this.useScanRange)
            {
                rangeSqr = entity.scanRange * entity.scanRange;
            }
            else if (this.useAttackRange)
            {
                rangeSqr = entity.attackRange * entity.attackRange;
            }
            else
            {
                rangeSqr = this.customRange * this.customRange;
            }

            // iterate through all observations and apply relevant 'filters'
            for (int i = 0; i < count; i++)
            {
                var obs = observations[i];
                if (this.skipAllies && obs.entity.type == entity.type)
                {
                    continue;
                }

                if (this.entityType != EntityType.Any && obs.entity.type != this.entityType)
                {
                    continue;
                }

                if (this.onlyVisible && !obs.isVisible)
                {
                    continue;
                }

                if (rangeSqr > 0f)
                {
                    // only check distance if relevant (no relevance if distance is 0)
                    var distance = (obs.position - entity.position).sqrMagnitude;
                    if (distance > rangeSqr)
                    {
                        continue;
                    }
                }

                // if none of the filters apply to this specific observation, then return true (unless we are reversing the logic with 'not')
                return this.not ? 0f : this.score;
            }

            return this.not ? this.score : 0f;
        }
    }
}