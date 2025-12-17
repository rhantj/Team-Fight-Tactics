using UnityEngine;

public class BGMStarter : MonoBehaviour
{
    private void Start()
    {
        // 게임 시작 시 자동으로 BGM 재생
        SettingsUI.PlayBGM("BGM1", 1f);
    }
}
