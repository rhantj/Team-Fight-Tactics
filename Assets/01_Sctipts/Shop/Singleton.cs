using UnityEngine;

/// <summary>
/// 씬 단위 싱글톤(Singleton) 패턴을 제공하는 제네릭 베이스 클래스.
///
/// - DontDestroyOnLoad(DDOL)을 사용하지 않는다.
/// - 씬마다 하나의 인스턴스만 존재하도록 보장한다.
/// - 씬에 이미 배치된 오브젝트가 있으면 그것을 싱글톤으로 사용한다.
/// - 씬에 존재하지 않을 경우, 자동으로 GameObject를 생성하여 싱글톤으로 사용한다.
///
/// 전투 씬 / 준비 씬 등 씬 전환이 명확한 구조에서
/// 매니저 객체의 생명주기를 씬 단위로 관리하기 위한 용도로 설계되었다.
/// </summary>
/// <typeparam name="T">싱글톤으로 관리할 MonoBehaviour 타입</typeparam>
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
