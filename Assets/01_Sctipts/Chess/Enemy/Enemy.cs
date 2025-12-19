using UnityEngine;

public class Enemy : Chess
{

    protected override void Awake()
    {
        base.Awake();
        team = Team.Enemy;
    }
    private void Start()
    {
        transform.rotation = Quaternion.Euler(Vector3.up * 180f);

        var statusUI = GetComponentInChildren<ChessStatusUI>(); //12.19 add Kim
        if (statusUI != null)
        {
            statusUI.Bind(this);
        }
        else
        {
        }
    }
}
