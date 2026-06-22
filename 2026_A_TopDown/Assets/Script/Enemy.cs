using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("--- 몬스터 스텟 ---")]
    public float maxHp = 50f;
    private float currentHp;

    [Header("--- 전리품 세팅 ---")]
    public int creditReward = 10; // ★ 인스펙터창에서 이 숫자가 0이 아닌지 꼭 확인하세요!

    private bool isDead = false;

    void Start()
    {
        currentHp = maxHp;
    }

    // 플레이어의 공격을 받을 때 호출되는 함수
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"[몬스터 피격] 데미지: {damage} | 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("<color=red>[몬스터 사망 진입]</color> 이제 크레딧 지급을 시도합니다.");

        // ★ 새로 만든 PermanentCreditManager를 호출합니다.
        if (PermanentCreditManager.Instance != null)
        {
            PermanentCreditManager.Instance.AddCredits(creditReward);
        }
        else
        {
            // 하이어라키 창에 매니저 오브젝트를 만들지 않았다면 이 오류가 터집니다!
            Debug.LogError("[오류] 하이어라키 창에 PermanentCreditManager 오브젝트가 없거나 스크립트가 안 붙어있습니다!");
        }

        Destroy(gameObject);
    }
}