using UnityEngine;

public class ChestScript : MonoBehaviour, IEnemy
{
    Transform transform_;

    public EnemyId Id => EnemyId.Chest;
    public string Name => "Chest";
    public int Level => 1;
    public float Life => 1;
    public float LifePct => 1;
    public float MaxLife => 1;
    public bool IsDead => false;
    public void DoFlash(float amount, float ms) { }

    GameObject enemy;

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        gameObject.SetActive(false);

        var pos = transform_.position;
        LootDropScript.Instance.SpawnCoins(20, pos);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, pos, 5);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.GoldParticles, pos, 10);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, pos, 5);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, pos, 1);
        MapScript.Instance.TriggerExplosion(pos, 2, damageWallsOnly: false);

        bool spawnEnemy = Random.value < 0.5f;
        enemy.SetActive(spawnEnemy);
    }

    private void Start()
    {
        transform_ = transform;

        enemy = Enemies.Instance.CreateEnemy(EnemyId.SeekerScythe);
        enemy.SetActive(false);
        enemy.transform.SetPositionAndRotation(transform_.position + Vector3.down * 0.5f, Quaternion.identity);
    }
}
