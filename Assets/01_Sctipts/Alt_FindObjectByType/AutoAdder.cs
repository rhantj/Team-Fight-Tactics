using UnityEngine;

public abstract class AutoAdder<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        if (StaticRegistry<T>.Count > 0) return;
        StaticRegistry<T>.Add(this as T);
    }
    protected virtual void OnDisable()
    {
        StaticRegistry<T>.Remove(this as T);
    }
}