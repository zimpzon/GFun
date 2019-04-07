using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public GameObject Prefab;
    public int StartCount;
    public int ExpandCount = 10;

    public int PoolCount;
    public int InFlightCount;

    Queue<GameObject> queue_ = new Queue<GameObject>();

    private void Awake()
    {
        if (queue_.Count == 0)
            InstantiateNew(StartCount);
    }

    void InstantiateNew(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            var newObj = Instantiate(Prefab);
            newObj.transform.SetParent(transform);
            newObj.SetActive(false);
            queue_.Enqueue(newObj);
        }

        PoolCount = queue_.Count;
    }

    public GameObject GetFromPool()
    {
        PoolCount = queue_.Count;
        if (PoolCount == 0)
            InstantiateNew(ExpandCount);

        InFlightCount++;
        var go = queue_.Dequeue();
        return go;
    }

    public void ReturnToPool(GameObject go)
    {
        InFlightCount--;
        go.SetActive(false);
        queue_.Enqueue(go);
        PoolCount = queue_.Count;
    }
}
