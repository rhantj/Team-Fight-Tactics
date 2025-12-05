using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooltest : MonoBehaviour
{
    private void Start()
    {
        // 1) 10개 스폰
        for (int i = 0; i < 10; i++)
        {
            var obj = PoolManager.Instance.Spawn("Cube");
            obj.transform.position = new Vector3(i * 2, 0, 0);

            // 2초 뒤 풀에 자동 복귀
            StartCoroutine(AutoReturn(obj));
        }
    }

    private IEnumerator AutoReturn(GameObject obj)
    {
        yield return new WaitForSeconds(2f);
        obj.GetComponent<PooledObject>().ReturnToPool();
    }
}
