#pragma warning disable 1591

using UnityEngine;

namespace Apex.Examples.AI.Game
{
    public interface IEntity
    {
        EntityType AiType { get; }
        Vector3 Position { get; }
    }
}
