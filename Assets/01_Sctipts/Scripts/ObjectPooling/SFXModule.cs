using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class SFXModule : MonoBehaviour
{
    [SerializeField] AudioSource aus;
    PooledObject pooled;

    private void Awake()
    {
        if (TryGetComponent<PooledObject>(out var po))
            pooled = po;
    }

    private void OnEnable()
    {
        StartCoroutine(Co_Return());
    }

    IEnumerator Co_Return()
    {
        yield return null;
        while (aus.isPlaying)
        {
            yield return null;
        }

        if (aus.spatialBlend <= 0.9f)
            pooled.ReturnToPool();
    }

    public void Play(AudioClip clip, float volume = 1f, float spatialBlend = 0f)
    {
        aus.clip = clip;
        aus.volume = volume;
        aus.spatialBlend = spatialBlend;
        aus.loop = spatialBlend <= 0.9f;
        aus.Play();
    }
}