using UnityEngine;

[System.Serializable]
public class TraitSynergyIconSet
{
    public TraitType trait;
    public Sprite gray;
    public Sprite bronze;
    public Sprite silver;
    public Sprite gold;
}

[CreateAssetMenu(
    fileName = "TraitSynergyIconDatabase",
    menuName = "TFT/Trait Synergy Icon Database")]
public class TraitSynergyIconDatabase : ScriptableObject
{
    public TraitSynergyIconSet[] sets;

    public Sprite GetIcon(TraitType trait, int count)
    {
        foreach (var set in sets)
        {
            if (set.trait != trait) continue;

            if (count <= 1) return set.gray;
            if (count == 2) return set.bronze;
            if (count == 3) return set.silver;
            return set.gold;
        }

        return null;
    }
}
