using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class TrailModule : MonoBehaviour
{
    [SerializeField] float speed = 30f;

    private void Start()
    {
        VFXManager.Register(gameObject);
    }

    public void MoveTo(Vector3 target)
    {
        StartCoroutine(Co_MoveTo(target));
    }

    IEnumerator Co_MoveTo(Vector3 target)
    {
        var dist = (transform.position - target).sqrMagnitude;
        var delta = speed * Time.deltaTime;
        while (dist > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, delta);
            dist -= delta;
            yield return null;
        }

        var pooled = GetComponent<PooledObject>();
        pooled.ReturnToPool();
    }
}