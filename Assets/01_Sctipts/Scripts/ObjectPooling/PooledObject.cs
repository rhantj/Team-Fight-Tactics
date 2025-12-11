using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector]
    public string poolId;

    public void ReturnToPool()
    {
        if(!string.IsNullOrEmpty(poolId))
        {
            PoolManager.Instance.Despawn(poolId, gameObject);
        }
        else
        {
            Debug.LogWarning($"PooledObject '{name}' 풀 아이디 없음. ");
            Destroy(gameObject);
        }
    }
}
