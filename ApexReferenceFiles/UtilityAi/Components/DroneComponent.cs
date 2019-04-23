#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    public sealed class DroneComponent : EntityComponentBase
    {
        public override EntityType type
        {
            get { return EntityType.Drone; }
        }
    }
}