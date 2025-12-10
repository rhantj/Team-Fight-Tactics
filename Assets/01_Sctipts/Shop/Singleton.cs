using UnityEngine;

// DDOL이 적용되지 않은 버전입니다. 필요시 상속받은 클래스에서 별도로 처리해주세요.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance; //필드
    public static T Instance   //프로퍼티
    {
        get
        {
            if (instance == null)
            {
                // 이미 씬에 배치된 매니저 오브젝트가 있다면 싱글톤으로 사용
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    //그래도 없다면 새로 생성해서 싱글톤으로 사용
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    //씬에 이미 존재하는 싱글톤 오브젝트를 등록하기 위한 Awake
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
