namespace Assets.Scripts
{
    public interface IEffect
    {
        eEffects Effect { get; }
        float Value { get; }
        float Time { get; set; }
    }
}