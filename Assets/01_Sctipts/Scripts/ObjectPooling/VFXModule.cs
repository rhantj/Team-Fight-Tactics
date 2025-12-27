using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class VFXModule : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    PooledObject pooled;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (TryGetComponent<PooledObject>(out var po))
        {
            pooled = po;
        }
    }

    private void OnEnable()
    {
        VFXManager.Register(gameObject);
        ps.Play();
        StartCoroutine(Co_Return());
    }

    private void OnDisable()
    {
        VFXManager.Unregister(gameObject);
    }

    IEnumerator Co_Return()
    {
        yield return null;
        while (ps.IsAlive(true))
        {
            yield return null;
        }

        VFXManager.Unregister(gameObject);
        pooled.ReturnToPool();
    }
}
