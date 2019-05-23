using UnityEngine;

public interface IEnemy
{
    EnemyId Id { get; }
    string Name { get; }
    int Level { get; }
    float Life { get; }
    float LifePct { get; }
    float MaxLife { get; }
    int XP { get; }
    bool IsDead { get; }
    bool LootDisabled { get; }
    void DoFlash(float amount, float ms);
    void TakeDamage(int amount, Vector3 damageForce);
}
