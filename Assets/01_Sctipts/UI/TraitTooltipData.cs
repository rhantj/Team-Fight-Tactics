using UnityEngine;

[CreateAssetMenu(
    menuName = "Synergy/Trait Tooltip Data",
    fileName = "TraitTooltipData"
)]
public class TraitTooltipData : ScriptableObject
{
    public TraitType trait;

    [TextArea]
    public string description;

    [TextArea]
    public string countDescription;
}
