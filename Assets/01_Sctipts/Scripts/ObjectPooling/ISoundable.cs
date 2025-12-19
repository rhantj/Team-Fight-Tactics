using UnityEngine;
public interface ISoundable
{
    void PlaySound(string name, Vector3 pos, float volume = 1f, float spatialBlend = 0f);
    void SetBGMVolume(float volume);
    void SetSFXVolume(float volume);
}