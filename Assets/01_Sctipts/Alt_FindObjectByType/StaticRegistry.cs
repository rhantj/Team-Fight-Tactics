using System.Collections.Generic;
using UnityEditor;

// 전역 레지스트리 클래스
// FindObjectByType 대신 사용 가능
public static class StaticRegistry<T> where T : class
{
    private static HashSet<T> _inst = new();
    static T cach;

    public static int Count => _inst.Count;

    public static void Add(T inst)
    {
        if (inst != null)
        {
            _inst.Add(inst);

            cach ??= inst;
        }
    }

    public static void Remove(T inst)
    {
        if (inst != null)
            _inst.Remove(inst);

        if(ReferenceEquals(cach, inst))
        {
            cach = null;
            foreach (var item in _inst)
            {
                cach = item;
                break;
            }
        }
    }

    public static T Find()
    {
        if(cach != null) 
            return cach;

        foreach (var inst in _inst)
        {
            cach = inst;
            return inst;
        }

        return null;
    }
}
