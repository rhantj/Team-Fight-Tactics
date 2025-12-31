using UnityEngine;

public class ChessStarVFXPlayer : MonoBehaviour
{
    [SerializeField] private Chess owner;

    [Header("VFX Pool IDs")]
    [SerializeField] private string twoStarVFX;
    [SerializeField] private string threeStarVFX;

    [Header("SFX Keys")]
    [SerializeField] private string twoStarSFX;
    [SerializeField] private string threeStarSFX;

    [SerializeField] private Vector3 offset = Vector3.up * 1.5f;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        if (owner != null)
            owner.OnStarUp += OnStarUp;
    }

    private void OnDisable()
    {
        if (owner != null)
            owner.OnStarUp -= OnStarUp;
    }

    private void OnStarUp(int starLevel)
    {
        // ===== VFX =====
        string vfxId = starLevel switch
        {
            2 => twoStarVFX,
            3 => threeStarVFX,
            _ => null
        };

        if (!string.IsNullOrEmpty(vfxId))
        {
            var vfx = PoolManager.Instance.Spawn(vfxId);
            if (vfx != null)
            {
                vfx.transform.SetPositionAndRotation(
                    transform.position + offset,
                    Quaternion.identity
                );
            }
        }

        // ===== SFX =====
        string sfxKey = starLevel switch
        {
            2 => twoStarSFX,
            3 => threeStarSFX,
            _ => null
        };

        if (!string.IsNullOrEmpty(sfxKey))
        {
            SettingsUI.PlaySFX(
                sfxKey,
                transform.position,
                1f,
                1f
            );
        }
    }
}
