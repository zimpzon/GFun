#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Memory;
    using UnityEngine;

    public interface IAIEntity : IEntity
    {
        float CurrentNormalizedHealth { get; }

        Vector3 MoveTarget { get; }
        bool MoveTargetReached { get; }
        void MoveTo(Vector3 target);
        void StopMove();

        bool HasLOSToPlayer(float maxAge);
        Vector3 PlayerLatestSeenPosition { get; }
        Vector3 PlayerPosition { get; }

        bool HasNearbyCover { get; }
        Vector3 NearbyCoverPosition { get; }

        Vector3 GetRandomFreePosition();

        /// <summary>
        /// Receives a list of communicated memory observations and adds newer observations to own memory.
        /// </summary>
        /// <param name="observations">The observations.</param>
        void ReceiveCommunicatedMemory(IList<Observation> observations);
    }
}
