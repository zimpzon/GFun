using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance;

    GameObjectPool textPool_;

    void Awake()
    {
        Instance = this;
        textPool_ = GetComponentInChildren<GameObjectPool>();
    }

    public void Spawn(Vector3 position, string text, Color color, float speed = 1.0f, float timeToLive = 2.0f)
    {
        var go = textPool_.GetFromPool();
        var script = go.GetComponent<FloatingTextScript>();
        script.Init(textPool_, position, text, color, speed, timeToLive);
        go.SetActive(true);
    }
}
