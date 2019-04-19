public interface IEnemy
{
    EnemyIds Id { get; }
    string Name { get; }
    int Level { get; }
    float Life { get; }
    bool IsDead { get; }
    void DoFlash(float amount, float ms);
}
