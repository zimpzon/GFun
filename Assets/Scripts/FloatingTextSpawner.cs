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

    public void Spawn(Vector3 position, string text, float timeToLive = 2.0f)
    {
        var go = textPool_.GetFromPool();
        var script = go.GetComponent<FloatingTextScript>();
        script.Init(position, text, textPool_, timeToLive);
        go.SetActive(true);
    }
}
