using UnityEngine;

public class TestingCube : MonoBehaviour
{
    public void SetPosition(Vector3 position)
    {
        position.y = 1.5f;
        transform.position = position;
    }
}