#pragma warning disable 1591

using UnityEngine;

namespace Apex.Examples.AI.Game
{
    public interface IEntity
    {
        EntityType type { get; }
        Vector3 position { get; }
    }
}
