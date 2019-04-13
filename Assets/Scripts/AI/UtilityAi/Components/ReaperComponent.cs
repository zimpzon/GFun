#pragma warning disable 1591

namespace Apex.Examples.AI.Game
{
    public sealed class ReaperComponent : EntityComponentBase
    {
        public override EntityType type
        {
            get { return EntityType.Reaper; }
        }
    }
}