using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPart_Collider : MonoBehaviour, IDamageable
{
    [Header("메인 체력 스크립트 연결")]
    // 보스 최상위 오브젝트에 있는 '진짜 체력 스크립트'를 연결해 줘야 해.
    public EnemyHealth mainHealth;

    [Header("부위별 데미지 설정")]
    // 이 부위를 때렸을 때 데미지가 몇 배로 들어갈지 결정 (머리=2f, 꼬리=0.5f 등)
    public float damageMultiplier = 1f;

    public void TakeDamage(float damage, Vector3 hitPoint, float poiseDamage)
    {
        if (mainHealth != null)
        {
            float finalDamage = damage * damageMultiplier;

            mainHealth.TakeDamage(finalDamage, hitPoint, poiseDamage);

            Debug,Log($"[{gameObject.name}] 부위 피격! 배율: {damageMultiplier}x ➡️ 깎인 체력: {finalDamage}");
        }
        else
        {
            Debug.LogError($"{gameObject.name}에 본체 체력(EnemyHealth)이 연결되지 않았습니다!");
        }
    }
}
