using UnityEngine;

public class ChessVFXPlayer : MonoBehaviour
{
    [SerializeField] Chess owner;
    [SerializeField] string vfxName;
    GameObject vfxObj;
    Vector3 offset = Vector3.up * 1.5f;

    private void Awake()
    {
        owner = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        owner.OnAttack += VFXStart;
    }

    private void OnDisable()
    {
        owner.OnAttack -= VFXStart;
    }

    private void VFXStart()
    {
        if (vfxName == null) return;

        vfxObj = PoolManager.Instance.Spawn(vfxName);
        var target = owner.CurrentTarget;

        if (vfxObj == null) return;

        vfxObj.transform.SetPositionAndRotation(transform.position + offset, Quaternion.identity);
        var mod = vfxObj.GetComponent<TrailModule>();
        mod.MoveTo(target.transform.position + offset);
    }
}