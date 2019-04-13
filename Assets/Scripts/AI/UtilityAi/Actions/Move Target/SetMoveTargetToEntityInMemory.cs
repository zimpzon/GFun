#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using UnityEngine;

    public class SetMoveTargetToEntityInMemory : ActionWithOptions<GameObject>
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            // get all observations from memory
            var observations = c.memory.allObservations;
            if (observations.Count == 0)
            {
                return;
            }
            /*
            var best = this.GetBest(context, observations);
            if (best != null)
            {
                // set the attack target through method, property, field or any way desired
                c.memory.allObservations. = best;
            }*/
            
        }
    }

}


