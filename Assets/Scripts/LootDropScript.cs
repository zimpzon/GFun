using UnityEngine;

public class LootDropScript : MonoBehaviour
{
    public static LootDropScript Instance;

    public GameObjectPool CoinPool;
    public GameObjectPool HealthPool;

    public void SpawnDrops(IEnemy enemy, Vector3 position)
    {
        if (enemy.LootDisabled)
            return;

        SpawnCoins(enemy, position);
        SpawnHealth(enemy, position);
    }

    void SpawnHealth(IEnemy enemy, Vector3 position)
    {
        int count = Random.value < 0.1f ? 2 : 1;
        if (Random.value < 0.9f)
            count = 0;

        if (enemy.Id == EnemyId.Golem)
            count = 1;

        SpawnHealth(count, position);
    }

    void SpawnCoins(IEnemy enemy, Vector3 position)
    {
        int count = Random.Range(0, 5 + enemy.Level);
        if (enemy.Id == EnemyId.Golem)
            count = 20;

        SpawnCoins(count, position);
    }

    public void SpawnHealth(int count, Vector3 position, float power = 1.0f)
    {
        for (int i = 0; i < count; ++i)
        {
            var heart = HealthPool.GetFromPool();
            heart.transform.position = position;
            var heartScript = heart.GetComponent<AutoPickUpActorScript>();
            var randomDirection = Random.insideUnitCircle.normalized;
            float randomForce = (Random.value * 0.5f + 0.5f) * 2 * power;
            heartScript.ObjectPool = HealthPool;
            heart.gameObject.SetActive(true);
            heartScript.Throw(randomDirection * randomForce);
        }
    }

    public void SpawnCoins(int count, Vector3 position, float power = 1.0f)
    {
        for (int i = 0; i < count; ++i)
        {
            var coin = CoinPool.GetFromPool();
            coin.transform.position = position;
            var coinScript = coin.GetComponent<AutoPickUpActorScript>();
            var randomDirection = Random.insideUnitCircle.normalized;
            float randomForce = (Random.value * 0.5f + 0.5f) * 3 * power;
            coinScript.ObjectPool = CoinPool;
            coin.gameObject.SetActive(true);
            coinScript.Throw(randomDirection * randomForce);
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
