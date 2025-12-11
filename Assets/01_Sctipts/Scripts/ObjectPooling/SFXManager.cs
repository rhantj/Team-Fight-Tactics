using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Don't use Singleton pattern

spatialBlend == 0 : bgm
spatialBlend == 1 : other 
 */

public static class SoundSystem
{
    public static ISoundable SoundPlayer { get; private set; }
    public static void DI(ISoundable soundPlayer)
    {
        SoundPlayer = soundPlayer;
    }
}

public class SFXManager : MonoBehaviour, ISoundable
{
    [SerializeField] AudioClip[] preloadSFX;
    private Dictionary<string, AudioClip> clipDic = new();
    public List<GameObject> usingSound = new();

    private void Awake()
    {
        foreach(var clip in preloadSFX)
        {
            clipDic[clip.name] = clip;
        }

        SoundSystem.DI(this);
    }

    public void PlaySound(string name, Vector3 pos, float volume = 1f, float spatialBlend = 0f)
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
