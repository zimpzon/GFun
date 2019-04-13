#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    public sealed class GhostComponent : EntityComponentBase
    {
        public override EntityType type
        {
            get { return EntityType.Ghost; }
        }
    }
}