using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private readonly T prefab;
    private readonly Transform parent;
    private readonly string poolId;

    public int Count => pool.Count; // 재고 확인용 프로퍼티

    private readonly HashSet<T> activeSet = new HashSet<T>();


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

        //PooledObject 자동 부착해주는
        var pooled = obj.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = obj.gameObject.AddComponent<PooledObject>();

        pooled.poolId = poolId;

        return obj;
    }


    public T Get()
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateNewObject();
        }

        obj.gameObject.name = poolId;
        obj.gameObject.SetActive(true);

        activeSet.Add(obj);   // 사용중 등록

        return obj;
    }


    public void Release(T obj)
    {
        if (obj == null) return;

        if (activeSet.Remove(obj))   // 사용중 해제
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }


    public void ResetPool()
    {
        foreach (var active in activeSet)
        {
            if (active == null) continue;

            active.gameObject.SetActive(false);
            pool.Enqueue(active);
        }

        activeSet.Clear();
    }




}
