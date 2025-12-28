using System.Collections.Generic;
using UnityEngine;

public static class VFXManager
{
    private static readonly List<GameObject> vfxObjects = new();

    public static void Register(GameObject obj)
    {
        vfxObjects.Add(obj);
    }

    public static void Unregister(GameObject obj)
    {
        vfxObjects.Remove(obj);
    }

    public static void ClearAllVFX()
    {
        foreach(var vfx in vfxObjects)
        {
            if(vfx.TryGetComponent<VFXModule>(out var _))
            {
                var pooled = vfx.GetComponent<PooledObject>();
                pooled?.ReturnToPool();
            }
        }

        vfxObjects.Clear();
    }
}
