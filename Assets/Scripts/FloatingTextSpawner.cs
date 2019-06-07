using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance;

    GameObjectPool textPool_;
    Queue<GameObject> queue_ = new Queue<GameObject>();
    float nextQueueItem_;

    void Awake()
    {
        Instance = this;
        textPool_ = GetComponentInChildren<GameObjectPool>();
    }

    public void Spawn(Vector3 position, string text, Color color, float speed = 1.0f, float timeToLive = 2.0f, FontStyles fontStyle = FontStyles.Bold, bool useQueue = false)
    {
        var go = textPool_.GetFromPool();
        var script = go.GetComponent<FloatingTextScript>();
        script.Init(textPool_, position, text, color, speed, timeToLive, fontStyle);
        if (useQueue)
        {
            go.SetActive(false);
            queue_.Enqueue(go);
        }
        else
        {
            go.SetActive(true);
        }
    }

    public void Update()
    {
        if (queue_.Count > 0 && Time.unscaledTime > nextQueueItem_)
        {
            var go = queue_.Dequeue();
            go.SetActive(true);

            nextQueueItem_ = Time.unscaledTime + 0.5f;
        }
    }
}
