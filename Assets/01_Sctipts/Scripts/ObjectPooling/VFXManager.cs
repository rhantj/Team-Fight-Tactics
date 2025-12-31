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
        if (vfxObjects.Count <= 0) return;
        for (int i = 0; i < vfxObjects.Count; ++i)
        {
            var pooled = vfxObjects[i].GetComponent<PooledObject>();
            pooled.ReturnToPool();
        }

        vfxObjects.Clear();
    }
}
