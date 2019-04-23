public interface IEnemy
{
    EnemyId Id { get; }
    string Name { get; }
    int Level { get; }
    float Life { get; }
    bool IsDead { get; }
    void DoFlash(float amount, float ms);
}
