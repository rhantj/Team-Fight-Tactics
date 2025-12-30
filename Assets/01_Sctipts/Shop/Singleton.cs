using UnityEngine;

/// <summary>
/// 싱글톤(Singleton) 패턴을 제공하는 제네릭 베이스 클래스.
///
/// - 싱글톤의 공통 뼈대(Instance 관리, 중복 방지)를 제공한다.
/// - 기본적으로 DontDestroyOnLoad(DDOL)를 적용하지 않는다.
/// - DDOL이 필요한 경우, 파생 클래스에서 Awake()를 override하여
///   명시적으로 DontDestroyOnLoad를 호출하도록 설계되었다.
/// - 씬에 이미 배치된 오브젝트가 있으면 그것을 싱글톤으로 사용한다.
/// - 씬에 존재하지 않을 경우, 자동으로 GameObject를 생성하여 싱글톤으로 사용한다.
///
/// 즉, 이 클래스는
/// "DDOL을 강제하지도, 금지하지도 않는"
/// 싱글톤의 공통 베이스 역할을 한다.
/// </summary>
/// <typeparam name="T">싱글톤으로 관리할 MonoBehaviour 타입</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance; //필드
    private static bool isQuitting; //앱 종료중 인스턴스 접근 방지 위해 추가
    public static T Instance   //프로퍼티
    {
        get
        {
            if (isQuitting) return null;
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

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

}
