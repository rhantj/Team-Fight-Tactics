using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private readonly T prefab;
    private readonly Transform parent;
    private readonly string poolId;

    public int Count => pool.Count; // 재고 확인용 프로퍼티

    public ObjectPool(T prefab, int preloadCount, string poolId, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.poolId = poolId;

        for(int i = 0; i< preloadCount; i++)
        {
            T obj = CreateNewObject();
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private T CreateNewObject()
    {
        T obj = GameObject.Instantiate(prefab, parent);
        obj.gameObject.name = poolId;
        return obj;
    }

    public T Get()
    {
        if(pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.name = poolId;
            obj.gameObject.SetActive(true);
            return obj;
        }

        return CreateNewObject();
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
