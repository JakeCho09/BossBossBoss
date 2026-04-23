using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Image bossHealthBar; // 화면에 빨갛게 줄어들 체력바 이미지

    [Header("데이터 연결")]
    public EnemyHealth bossHealth; // 씬에 있는 샌드백(몬스터)

    private void Start()
    {
        // 몬스터가 할당되어 있다면, 몬스터의 '방송국(OnHealthChanged)'을 구독!
        // "네 체력 변할 때마다 내 UpdateHealthBar 함수도 같이 실행해줘!" 라는 뜻이야.
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    // 매 프레임 도는 Update()가 아니라, 몬스터가 맞을 때만 딱! 실행되는 효율적인 함수
    private void UpdateHealthBar(float currentHp, float maxHp)
    {
        // Image의 Fill Amount 속성은 0~1 사이의 값만 받기 때문에 백분율로 나눠줌
        bossHealthBar.fillAmount = currentHp / maxHp;
    }

    private void OnDestroy()
    {
        // 씬이 넘어가거나 UI가 꺼질 때, 구독을 취소(-=) 안 해주면 
        // 없는 함수를 찾으려다가 메모리 릭(에러)이 발생해. 프로들의 필수 방어 코드야!
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
