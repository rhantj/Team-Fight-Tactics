using UnityEngine;
using System.Collections;

public class CaitlynSkill_Q : SkillBase
{
    public override IEnumerator Execute(ChessStateBase caster)
    {
        Debug.Log("히히 케틀 Q발사");
        yield return null;
    }
}
