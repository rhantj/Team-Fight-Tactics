using UnityEngine;

public class ChessStarVFXPlayer : MonoBehaviour
{
    [SerializeField] private Chess owner;

    [Header("VFX Pool IDs")]
    [SerializeField] private string twoStarVFX;
    [SerializeField] private string threeStarVFX;

    [SerializeField] private Vector3 offset = Vector3.up * 1.5f;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        if (owner != null)
            owner.OnStarUp += PlayStarVFX;
    }

    private void OnDisable()
    {
        if (owner != null)
            owner.OnStarUp -= PlayStarVFX;
    }

    private void PlayStarVFX(int starLevel)
    {
        string vfxId = starLevel switch
        {
            2 => twoStarVFX,
            3 => threeStarVFX,
            _ => null
        };

        if (string.IsNullOrEmpty(vfxId))
            return;

        var vfx = PoolManager.Instance.Spawn(vfxId);
        if (vfx == null) return;

        vfx.transform.SetPositionAndRotation(
            transform.position + offset,
            Quaternion.identity
        );
    }
}
