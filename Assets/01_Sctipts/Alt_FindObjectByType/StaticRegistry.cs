using System.Collections.Generic;

// 전역 레지스트리 클래스
// FindObjectByType 대신 사용 가능
public static class StaticRegistry<T> where T : class
{
    private static HashSet<T> _inst = new();

    public static void Add(T inst)
    {
        if (inst != null)
            _inst.Add(inst);
    }

    public static void Remove(T inst)
    {
        if (inst != null)
            _inst.Remove(inst);
    }

    public static T Find()
    {
        foreach (var inst in _inst)
        {
            return inst;
        }
        return null;
    }
}
