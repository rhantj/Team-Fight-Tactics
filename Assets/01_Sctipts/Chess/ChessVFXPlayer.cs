using UnityEngine;

public class ChessVFXPlayer : MonoBehaviour
{
    [SerializeField] Chess owner;
    [SerializeField] string vfxName;
    GameObject vfxObj;
    Vector3 offset = Vector3.up * 1.5f;

    //kim add
    [Header("Spawn Point")]
    [SerializeField] private Transform firePoint;        
    [SerializeField] private Vector3 localOffset = Vector3.up * 1.5f;

    [Header("Gizmo")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private float gizmoRadius = 0.08f;

    private void Awake()
    {
        owner = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        //owner.OnAttack += VFXStart;
        owner.OnShoot += VFXStart;
    }

    private void OnDisable()
    {
        //owner.OnAttack -= VFXStart;
        owner.OnShoot -= VFXStart;
    }


    private Vector3 GetSpawnPos()
    {
        if (firePoint != null) return firePoint.position;
        return transform.TransformPoint(localOffset);
    }
    private void VFXStart()
    {
        if (string.IsNullOrEmpty(vfxName)) return;

        var target = owner.CurrentTarget;
        if (target == null) return;

        var vfxObj = PoolManager.Instance.Spawn(vfxName);
        if (vfxObj == null) return;

        Vector3 spawnPos = GetSpawnPos();
        vfxObj.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

        var mod = vfxObj.GetComponent<TrailModule>();
        if (mod != null)
            mod.MoveTo(target.transform.position + Vector3.up * 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.TransformPoint(localOffset);
        Gizmos.DrawWireSphere(spawnPos, gizmoRadius);
        Gizmos.DrawLine(spawnPos, spawnPos + transform.forward * 0.4f);
    }
}