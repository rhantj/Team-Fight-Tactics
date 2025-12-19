using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public string poolId;
    [HideInInspector] public UnityEngine.Component releaseComponent;

    public void ReturnToPool()
    {
        if (PoolManager.Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        PoolManager.Instance.Despawn(gameObject);
    }
}
