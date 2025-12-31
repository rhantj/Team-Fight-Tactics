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
    [SerializeField] List<AudioClip> preloadSFX = new();    // 효과음 캐싱
    private Dictionary<string, AudioClip> clipDic = new();  // 효과음 분류
    public List<GameObject> usingSound = new();             // 사용중인 사운드

    private AudioSource currentBGMSource;  //12-17 Won Add

    private void Awake()
    {
        foreach(var clip in preloadSFX)
        {
            clipDic[clip.name] = clip;
        }

        SoundSystem.DI(this);
    }

    // 사운드 실행(클립이름, 위치, 볼륨, 2d/3d)
    public void PlaySound(string name, Vector3 pos, float volume = 1f, float spatialBlend = 1f)
    {
        if (!clipDic.ContainsKey(name)) return;

        // =========================
        // BGM 처리 추가 12-19 Won Add
        // =========================
        if (spatialBlend < 0.9f)
        {
            // 이전 BGM이 있으면 반드시 정지
            if (currentBGMSource != null)
            {
                currentBGMSource.Stop();

                // 풀 오브젝트라면 반환
                var pooled = currentBGMSource.GetComponent<PooledObject>();
                if (pooled != null)
                    pooled.ReturnToPool();
            }
        }

        var obj = PoolManager.Instance.Spawn("SFX Object");
        if (spatialBlend >= 0.9f) usingSound.Add(obj);
        obj.transform.position = pos;

        var sfx = obj.GetComponent<SFXModule>();
        var clip = clipDic[name];
        sfx.Play(clip, volume, spatialBlend);

        // BGM 인 경우
        if (spatialBlend < 0.9f)
        {
            currentBGMSource = obj.GetComponent<AudioSource>();
        }
    }

    // 키 접근이 아닌 클립접근용 메서드 하나 추가
    public void PlaySound(AudioClip clip, Vector3 pos, float volume = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return;

        // =========================
        // BGM 처리 (기존 로직 재사용)
        // =========================
        if (spatialBlend < 0.9f)
        {
            if (currentBGMSource != null)
            {
                currentBGMSource.Stop();

                var pooled = currentBGMSource.GetComponent<PooledObject>();
                if (pooled != null)
                    pooled.ReturnToPool();
            }
        }

        var obj = PoolManager.Instance.Spawn("SFX Object");
        if (spatialBlend >= 0.9f)
            usingSound.Add(obj);

        obj.transform.position = pos;

        var sfx = obj.GetComponent<SFXModule>();

        sfx.Play(clip, volume, spatialBlend);

        // BGM 인 경우
        if (spatialBlend < 0.9f)
        {
            currentBGMSource = obj.GetComponent<AudioSource>();
        }
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

    // 브금 볼륨 조절 
    public void SetBGMVolume(float volume)
    {
        if (currentBGMSource == null) return;
        currentBGMSource.volume = volume;
    }

    // SFX 볼륨 조절
    public void SetSFXVolume(float volume)
    {
        for (int i = usingSound.Count - 1; i >= 0; i--)
        {
            var obj = usingSound[i];

            if (obj == null)
            {
                usingSound.RemoveAt(i);
                continue;
            }

            var src = obj.GetComponent<AudioSource>();
            if (src != null)
            {
                src.volume = volume;
            }
        }
    }


}
