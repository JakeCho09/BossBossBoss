using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    [Header("무기 스팩")]
    public float weaponDamage = 20f;
    public float poiseDamage = 10f; // 강인도 감쇄력

    [Header("타격 판정 설정")]
    public Transform hitBoxCenter;
    public Vector3 hitBoxSize = new Vector3(0.5f, 1f, 0.5f);
    public LayerMask enemyLayer;

    private bool isAttacking = false;

    private HashSet<Collider> alreadeyHitEnemies = new HashSet<Collider>();

    public void EnabledWeapon()
    {
        isAttacking = true;
        alreadeyHitEnemies.Clear();
    }

    public void DisableWeapon()
    {
        isAttacking = false;
    }

    private void Update()
    {
        if (!isAttacking) return;

        Collider[] hitColliders = Physics.OverlapBox(hitBoxCenter.position, hitBoxSize / 2f, hitBoxCenter.rotation, enemyLayer);
        
        foreach (Collider hit in hitColliders)
        {
            if (alreadeyHitEnemies.Contains(hit)) continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Vector3 hitPoint = hit.ClosestPoint(hitBoxCenter.position);

                damageable.TakeDamage(weaponDamage, hitPoint, poiseDamage);

                alreadeyHitEnemies.Add(hit);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (hitBoxCenter == null) return;

        Gizmos.color = isAttacking ? Color.red : Color.green;

        Gizmos.matrix = Matrix4x4.TRS(hitBoxCenter.position, hitBoxCenter.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
    }

}
