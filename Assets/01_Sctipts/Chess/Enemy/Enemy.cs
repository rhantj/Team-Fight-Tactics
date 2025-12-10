using UnityEngine;

public class Enemy : ChessStateBase
{
    private void Start()
    {
        transform.rotation = Quaternion.Euler(Vector3.up * 180f);
    }
}
