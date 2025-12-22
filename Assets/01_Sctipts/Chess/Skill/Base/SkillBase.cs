using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    public bool endByAnimEvent = true;
    public bool blockUpdateWhileCasting = true;
    //=====
    public int repeatCount = 1;  
    public float repeatInterval = 0f;    

    public abstract IEnumerator Execute(ChessStateBase caster);
}
