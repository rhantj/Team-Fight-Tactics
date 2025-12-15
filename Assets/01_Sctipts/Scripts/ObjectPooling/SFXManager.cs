using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Don't use Singleton pattern

spatialBlend == 0 : bgm(2d sound)
spatialBlend == 1 : other(3d sound)
 */

// 실질적으로 외부에서 사용할 사운드 시스템
public static class SoundSystem
{
    // SoundSystem.SoundPlayer.Play(...)로 싱글톤처럼 사용 가능
    public static ISoundable SoundPlayer { get; private set; }
    public static void DI(ISoundable soundPlayer)
    {
        SoundPlayer = soundPlayer;
    }
}

public class SFXManager : MonoBehaviour, ISoundable
{
    [SerializeField] AudioClip[] preloadSFX;                // 효과음 캐싱
    private Dictionary<string, AudioClip> clipDic = new();  // 효과음 분류
    public List<GameObject> usingSound = new();             // 사용중인 사운드

    private void Awake()
    {
        foreach(var clip in preloadSFX)
        {
            clipDic[clip.name] = clip;
        }

        SoundSystem.DI(this);
    }

    // 사운드 실행(클립이름, 위치, 볼륨, 2d/3d)
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

    // 씬이 변경되거나 bgm 종료 시 사용
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
