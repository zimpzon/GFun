#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Memory;

    public interface IAIEntity : IEntity
    {
        void MoveToCover();
        void MoveToPlayer();
        void MoveToPlayerLatestSeenPosition();
        void MoveToRandomNearbyPosition();
        void FleeFromPlayer();
        void StopMove();

        float TimeSinceLatestMoveCommand { get; }
        bool HasLOSToPlayer(float maxAge);
        float CurrentNormalizedHealth { get; }
        bool MoveTargetReached { get; }
        bool HasNearbyCover { get; }

        /// <summary>
        /// Receives a list of communicated memory observations and adds newer observations to own memory.
        /// </summary>
        /// <param name="observations">The observations.</param>
        void ReceiveCommunicatedMemory(IList<Observation> observations);
    }
}
