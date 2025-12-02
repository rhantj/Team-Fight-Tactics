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
    public List<PoolConfig> pollConfigs = new List<PoolConfig>();
    private Dictionary<string, ObjectPool<Component>> poolDict = new Dictionary<string, ObjectPool<Component>>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
