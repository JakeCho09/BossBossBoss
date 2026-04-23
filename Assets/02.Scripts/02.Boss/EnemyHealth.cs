using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;

    // 옵저버 패턴의 핵심! UI에게 체력 변화를 알리는 '방송국' 역할
    // Action<현재체력, 최대체력> 형태로 데이터를 넘겨줄 거야.
    public event Action<float, float> OnHealthChanged;

    private void Start()
    {
        currentHealth = maxHealth;

        // 게임 시작 시, UI 체력바를 꽉 찬 상태로 초기화하기 위해 첫 방송을 쏴줌
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage, Vector3 hitPoint, float poiseDamage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[피격] 위치: {hitPoint} | 받은 데미지: {damage} | 남은 체력: {currentHealth}");

        // TODO: 나중에 hitPoint 위치에 피격 파티클이나 피 튀기는 이펙트 생성!
        // TODO: 강인도(poiseDamage) 누적 로직 추가해서 일정 이상 깎이면 그로기 상태 만들기!

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("으앙 쥬금");
        // TODO: 사망 애니메이션(Ragdoll 등) 재생 및 충돌체 꺼주기
        // 당장은 테스트를 위해 2초 뒤에 오브젝트 삭제
        Destroy(gameObject, 2f);
    }
}


