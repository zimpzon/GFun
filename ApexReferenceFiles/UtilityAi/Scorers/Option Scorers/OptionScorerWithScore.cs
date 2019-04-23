#pragma warning disable 1591

namespace Apex.Examples.AI
{
    using Apex.AI;
    using Apex.Serialization;

    /// <summary>
    /// An OptionScorerBase-deriving base class which simply adds a 'Score' field for inputting a score in the AI Editor inspector.
    /// </summary>
    /// <typeparam name="T">The type for the option scorer to operate on.</typeparam>
    public abstract class OptionScorerWithScore<T> : OptionScorerBase<T>
    {
        [ApexSerialization, FriendlyName("Score", "How much this scorer can score at maximum")]
        public float score = 0f;
    }
}