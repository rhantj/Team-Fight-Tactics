using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public string id;               //풀 이름
        public GameObject prefab;       //풀에서 생성할 프리펩
        public int preloadCount = 10;   //미리 만들 갯수
    }

    [Header("Pool Settig")]
    public List<PoolConfig> poolConfigs = new List<PoolConfig>();
    private Dictionary<string, ObjectPool<Component>> poolDict = new Dictionary<string, ObjectPool<Component>>();

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
        foreach(var config in poolConfigs)
        {
            if(string.IsNullOrEmpty(config.id)|| config.prefab == null)
            {
                Debug.LogWarning($"Invalid PoolConfig : {config.id}");
                continue;
            }

            Component component = config.prefab.GetComponent<Component>();

            var pool = new ObjectPool<Component>(
                component,
                config.preloadCount,
                transform);

            poolDict.Add(config.id, pool);
        }
    }

    //풀 꺼내기
    public GameObject Spawn(string id)
    {
        if (!poolDict.TryGetValue(id, out var pool))
        {
            Debug.LogError($"Pool ID '{id}' 없음");
            return null;
        }

        Component compnent = pool.Get();
        GameObject obj = compnent.gameObject;

        var pooledObj = obj.GetComponent<PooledObject>();

        if(pooledObj != null)
        {
            pooledObj.poolId = id;
        }

        return obj;
    }

    //풀 집어넣기
    public void Despawn(string id, GameObject obj)
    {
        if(!poolDict.TryGetValue(id, out var pool))
        {
            Debug.LogError($"Pool ID '{id}' 없음");
            Destroy(obj);
            return;
        }

        pool.Release(obj.GetComponent<Component>());
    }
}
