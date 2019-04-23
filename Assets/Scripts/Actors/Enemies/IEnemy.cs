public interface IEnemy
{
    EnemyId Id { get; }
    string Name { get; }
    int Level { get; }
    float Life { get; }
    float MaxLife { get; }
    bool IsDead { get; }
    void DoFlash(float amount, float ms);
}
