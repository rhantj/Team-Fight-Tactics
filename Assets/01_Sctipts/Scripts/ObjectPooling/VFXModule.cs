using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class VFXModule : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    PooledObject pooled;

    private void Awake()
    {
        if(TryGetComponent<PooledObject>(out var po))
        {
            pooled = po;
        }
    }

    private void OnEnable()
    {
        ps.Play();
        StartCoroutine(Co_Return());
    }

    IEnumerator Co_Return()
    {
        yield return null;
        while (ps.IsAlive(true))
        {
            yield return null;
        }

        pooled.ReturnToPool();
    }
}
