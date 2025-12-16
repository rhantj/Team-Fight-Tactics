using UnityEngine;

public class BattleStartButton : MonoBehaviour
{
    public void OnClickBattleStart()
    {
        Debug.Log("Battle Start Button Clicked");
        GameManager.Instance.RequestStartBattle();
    }
}
