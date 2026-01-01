using UnityEngine;

public class BasicAttackSFXListener : MonoBehaviour
{
    [SerializeField]
    private string basicAttackSfxName;

    private Chess chess;

    private void Awake()
    {
        chess = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        if (chess != null)
            chess.OnBasicAttackHit += Play;
    }

    private void OnDisable()
    {
        if (chess != null)
            chess.OnBasicAttackHit -= Play;
    }

    private void Play()
    {
        if (string.IsNullOrEmpty(basicAttackSfxName))
            return;

        SettingsUI.PlaySFX(
            basicAttackSfxName,
            transform.position,
            1f,
            1f
        );
    }
}
