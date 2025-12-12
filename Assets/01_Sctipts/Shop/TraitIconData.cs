using UnityEngine;

[System.Serializable]
public class TraitIconEntry
{
    public TraitType trait;
    public Sprite icon;
    public string displayName;
}

[CreateAssetMenu(fileName = "TraitIconDatabase", menuName = "TFT/Trait Icon Database")]
public class TraitIconDatabase : ScriptableObject
{
    public TraitIconEntry[] entries;

    public Sprite GetIcon(TraitType trait)
    {
        foreach (var e in entries)
        {
            if (e.trait == trait)
                return e.icon;
        }
        return null;
    }
    public string GetDisplayName(TraitType trait)
    {
        foreach (var e in entries)
        {
            if (e.trait == trait)
                return string.IsNullOrEmpty(e.displayName)
                    ? trait.ToString()
                    : e.displayName;
        }
        return trait.ToString();
    }
}
