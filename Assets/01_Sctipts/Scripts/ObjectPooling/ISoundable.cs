using UnityEngine;
public interface ISoundable
{
    void PlaySound(string name, Vector3 pos, float volume = 1f, float spatialBlend = 1f);
    void PlaySound(AudioClip clip, Vector3 pos, float volume = 1f, float spatialBlend = 1f);
    void SetBGMVolume(float volume);
    void SetSFXVolume(float volume);
}