using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    // SINGLETON

    // spatialBlend == 0 : bgm
    // spatialBlend == 1 : other

    [SerializeField] AudioClip[] preloadSFX;
    private Dictionary<string, AudioClip> clipDic = new();
    public List<GameObject> usingSound = new();

    private void Awake()
    {
        foreach(var clip in preloadSFX)
        {
            clipDic[clip.name] = clip;
        }
    }

    private void Start()
    {
        StartCoroutine(Co_SpawnSfx());
    }

    IEnumerator Co_SpawnSfx()
    {
        yield return null;
        PlaySfx("BGM1", Vector3.zero, .5f, 0);
        yield return new WaitForSeconds(1f);

        PlaySfx("Darius_Normal_Hit1", Vector3.zero, 1f, 0);
        yield return new WaitForSeconds(1f);

        PlaySfx("Jarvan_Normal_Hit1", Vector3.zero, 1f, 0);
        yield return new WaitForSeconds(1f);

        PlaySfx("Xayah_Normal_Hit", Vector3.zero, 1f, 0);
        yield return new WaitForSeconds(1f);
    }

    public void PlaySfx(string name, Vector3 pos, float volume = 1f, float spatialBlend = 0f)
    {
        if (!clipDic.ContainsKey(name)) return;

        var obj = PoolManager.Instance.Spawn("SFX Object");
        if (spatialBlend >= 0.9f) usingSound.Add(obj);
        obj.transform.position = pos;

        var sfx = obj.GetComponent<SFXModule>();
        var clip = clipDic[name];
        sfx.Play(clip, volume, spatialBlend);
    }

    public void StopAllSound()
    {
        foreach(var audio in usingSound)
        {
            var src = audio.GetComponent<AudioSource>();
            var pooled = audio.GetComponent<PooledObject>();

            src.Stop();
            pooled.ReturnToPool();
        }

        usingSound.Clear();
    }
}
