using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance;

    public GameObjectPool TextPool;

    void Awake()
    {
        Instance = this;    
    }

    public void Spawn(Vector3 position, string text, float timeToLive = 2.0f)
    {
        var go = TextPool.GetFromPool();
        var script = go.GetComponent<FloatingTextScript>();
        script.Init(position, text, timeToLive);
        go.SetActive(true);
    }
}
