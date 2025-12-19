using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    public abstract IEnumerator Execute(ChessStateBase caster);
}
