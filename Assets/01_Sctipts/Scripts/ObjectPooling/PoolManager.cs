using System.Collections;
using System.Collections.Generic;
//using System.ComponentModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public string id;               //풀 이름
        public AssetReferenceGameObject prefab;       //풀에서 생성할 프리펩
        public int preloadCount = 10;   //미리 만들 갯수
    }

    [Header("Pool Settig")]
    public List<PoolConfig> poolConfigs = new List<PoolConfig>();
    private Dictionary<string, ObjectPool<UnityEngine.Component>> poolDict
        = new Dictionary<string, ObjectPool<UnityEngine.Component>>();

    private bool isInitialized = false;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializePools();
    }

    //풀 준비(초기화)
    private void InitializePools()
    {
        if (isInitialized)
            return;

        isInitialized = true;

        foreach (var config in poolConfigs)
        {
            if (string.IsNullOrEmpty(config.id) || config.prefab == null)
                continue;

            AsyncOperationHandle<GameObject> handle =
                config.prefab.LoadAssetAsync<GameObject>();

            GameObject loadedPF = handle.WaitForCompletion();
            if (!loadedPF) continue;

            var component = loadedPF.GetComponent<Component>();

            var pool = new ObjectPool<Component>(
                component,
                config.preloadCount,
                config.id,
                transform
            );

            poolDict.Add(config.id, pool);
        }
    }


    //풀 꺼내기
    public GameObject Spawn(string id)
    {
        if (!poolDict.TryGetValue(id, out var pool))
        {
            //Debug.LogError($"Pool ID '{id}' 없음");
            return null;
        }

        UnityEngine.Component compnent = pool.Get();
        GameObject obj = compnent.gameObject;

        var pooledObj = obj.GetComponent<PooledObject>();
        if (pooledObj == null)
            pooledObj = obj.AddComponent<PooledObject>();

        pooledObj.poolId = id;
        pooledObj.releaseComponent = compnent;

        return obj;
    }

    //풀 집어넣기
    public void Despawn(string id, GameObject obj)
    {
        if (!poolDict.TryGetValue(id, out var pool))
        {
            //Debug.LogError($"Pool ID '{id}' 없음");
            Destroy(obj);
            return;
        }

        var pooledObj = obj.GetComponent<PooledObject>();
        if (pooledObj != null && pooledObj.releaseComponent != null)
        {
            pool.Release(pooledObj.releaseComponent);
            return;
        }

        Destroy(obj);
    }

    public int GetRemainCount(string id)
    {
        if (!poolDict.TryGetValue(id, out var pool))
        {
            //Debug.LogError($"Pool ID '{id}' 없음");
            return -1;
        }

        return pool.Count;
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        var pooled = obj.GetComponent<PooledObject>();
        if (pooled == null || string.IsNullOrEmpty(pooled.poolId))
        {
            Destroy(obj);
            return;
        }

        Despawn(pooled.poolId, obj);
    }

    public void ResetAllPools()
    {
        foreach (var pool in poolDict.Values)
        {
            pool.ResetPool();
        }
    }


}
