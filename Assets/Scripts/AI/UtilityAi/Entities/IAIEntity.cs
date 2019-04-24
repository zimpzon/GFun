#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    using System.Collections.Generic;
    using Memory;
    using UnityEngine;

    public interface IAIEntity : IEntity
    {
        float CurrentNormalizedHealth { get; }
        bool HasLOSToPlayer(float maxAge);
        Vector3 PlayerLatestSeenPosition { get; }
        Vector3 PlayerPosition { get; }
        Vector3 MoveTarget { get; }
        bool MoveTargetReached { get; }
        void MoveTo(Vector3 target);
        void StopMove();

        /// <summary>
        /// Receives a list of communicated memory observations and adds newer observations to own memory.
        /// </summary>
        /// <param name="observations">The observations.</param>
        void ReceiveCommunicatedMemory(IList<Observation> observations);
    }
}
