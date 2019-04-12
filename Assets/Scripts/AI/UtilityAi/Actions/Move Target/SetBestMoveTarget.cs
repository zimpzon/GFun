#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;

    /// <summary>
    /// This AI action finds and sets the best move target by evaluating all the sampled positions by calculating the score for each position through the list of scorers on this action.
    /// </summary>
    [FriendlyName("AI Example Set Best Move Target")]
    public sealed class SetBestMoveTarget : SetMoveTargetBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            // get the highest scoring position based on the list of scorers attached to this action
            var best = this.GetBest(context, c.sampledPositions);
            if (best.sqrMagnitude == 0f)
            {
                // no valid position found
                return;
            }

            // Set the identified best position as the entity's move target
            c.entity.moveTarget = best;
        }
    }
}