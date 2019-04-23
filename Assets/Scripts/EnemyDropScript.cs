using UnityEngine;

public class EnemyDropScript : MonoBehaviour
{
    public static EnemyDropScript Instance;

    public GameObjectPool CoinPool;
    public GameObjectPool HealthPool;

    public void SpawnDrops(IEnemy enemy, Vector3 position)
    {
        SpawnCoins(enemy, position);
        SpawnHealth(enemy, position);
    }

    void SpawnHealth(IEnemy enemy, Vector3 position)
    {
        if (Random.value < 0.9f)
            return;

        int count = Random.value < 0.2f ? 2 : 1;
        for (int i = 0; i < count; ++i)
        {
            var heart = HealthPool.GetFromPool();
            heart.transform.position = position;
            var heartScript = heart.GetComponent<AutoPickUpActorScript>();
            var randomDirection = Random.insideUnitCircle.normalized;
            float randomForce = (Random.value * 0.5f + 0.5f) * 2;
            heartScript.ObjectPool = HealthPool;
            heart.gameObject.SetActive(true);
            heartScript.Throw(randomDirection * randomForce);
        }
    }

    void SpawnCoins(IEnemy enemy, Vector3 position)
    {
        int count = Random.Range(0, 5 + enemy.Level);
        for (int i = 0; i < count; ++i)
        {
            var coin = CoinPool.GetFromPool();
            coin.transform.position = position;
            var coinScript = coin.GetComponent<AutoPickUpActorScript>();
            var randomDirection = Random.insideUnitCircle.normalized;
            float randomForce = (Random.value * 0.5f + 0.5f) * 3;
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
