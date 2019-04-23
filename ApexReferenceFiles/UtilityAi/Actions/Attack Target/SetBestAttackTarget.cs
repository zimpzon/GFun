#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Game;
    using Utilities;

    /// <summary>
    /// This AI action evaluates a list of options for best attack target, and sets the one scoring the highest
    /// </summary>
    public sealed class SetBestAttackTarget : ActionWithOptions<IEntity>
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var entity = c.entity;

            // get all observations from memory
            var observations = c.memory.allObservations;
            var count = observations.Count;
            if (count == 0)
            {
                // no available observations
                return;
            }

            // Get a list from the list buffer pool to reduce GC allocation and re-use memory
            var list = ListBufferPool.GetBuffer<IEntity>(5);

            // Loop through all observed entities in order to prepare a list of potential attack targets
            for (int i = 0; i < count; i++)
            {
                var obs = observations[i];
                var ent = obs.entity;
                if (ent.type == entity.type)
                {
                    // Cannot attack other entities of same type as self (they are allied)
                    continue;
                }

                list.Add(ent);
            }

            // get the highest scoring attack target after looping through the list of scorers attached to this action and scoring each element
            var best = this.GetBest(context, list);
            if (best != null)
            {
                // Set the attack target
                entity.attackTarget = best;
            }

            // Return the list buffer pool, so that the memory can be reused by other entities or AI elements
            ListBufferPool.ReturnBuffer(list);
        }
    }
}