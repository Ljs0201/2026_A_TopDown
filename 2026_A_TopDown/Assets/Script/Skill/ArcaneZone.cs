using UnityEngine;
using System.Collections.Generic;

public class ArcaneZone : SkillData
{
    [Header("--- 아케인 존 밸런스 세팅 ---")]
    public float baseDamage = 5f;
    public float damagePerLevel = 3f;
    public float damageInterval = 0.5f;
    public float slowMultiplier = 0.5f;

    private float timer = 0f;

    // 범위 내에 들어온 적들의 Collider2D를 담아두는 리스트
    private List<Collider2D> targetsInZone = new List<Collider2D>();

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    void Awake()
    {
        skillType = SkillType.ArcaneZone; // 에넘 타입 설정 (1번 인덱스)
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        // 0레벨(잠금)일 때는 그림과 충돌창을 끄고 연산을 생략합니다.
        if (currentLevel <= 0)
        {
            if (spriteRenderer != null && spriteRenderer.enabled) spriteRenderer.enabled = false;
            if (circleCollider != null && circleCollider.enabled) circleCollider.enabled = false;
            return;
        }

        // 1레벨 이상(해금)이 되면 그림과 충돌창을 활성화합니다.
        if (spriteRenderer != null && !spriteRenderer.enabled) spriteRenderer.enabled = true;
        if (circleCollider != null && !circleCollider.enabled) circleCollider.enabled = true;

        // [기획안 공식 반영] 1cell = 0.5f 기준 / 레벨업 시 1cell씩 크기 증가
        float cellCount = 3f + (currentLevel - 1) * 1.0f;
        float finalScale = cellCount * 0.5f;

        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // 0.5초마다 범위 내 적들에게 지속 데미지 주기
        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            timer = 0f;
            ApplyZoneDamage();
        }
    }

    private void ApplyZoneDamage()
    {
        float currentDamage = baseDamage + (currentLevel - 1) * damagePerLevel;
        float totalDamageThisFrame = 0f; // 이번 주기에 가한 총 데미지 합산용 변수

        for (int i = targetsInZone.Count - 1; i >= 0; i--)
        {
            // 리스트 검사 중 죽거나 씬에서 사라진 몬스터가 있다면 리스트에서 제거
            if (targetsInZone[i] == null || !targetsInZone[i].gameObject.activeSelf)
            {
                targetsInZone.RemoveAt(i);
                continue;
            }

            // ★ [버그 수정] Enemy -> 유저님의 적 스크립트인 EnemyAI로 컴포넌트를 가져옵니다.
            EnemyAI enemy = targetsInZone[i].GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);

                // 적에게 들어간 데미지만큼 합산용 변수에 누적
                totalDamageThisFrame += currentDamage;

                // 5레벨(만렙) 달성 시 실시간으로 슬로우 효과 부여 (해당 함수가 EnemyAI에 있어야 함)
                if (currentLevel >= 5)
                {
                    // 만약 EnemyAI에 ApplySlow 함수가 구현되어 있지 않다면 이 줄은 주석 처리(//) 하셔도 됩니다.
                    // enemy.ApplySlow(slowMultiplier); 
                }
            }
        }

        // ★ [데미지 실시간 JSON 누적] 아케인존은 1번 인덱스 슬롯을 사용합니다.
        if (totalDamageThisFrame > 0f && GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 1)
        {
            // 1번 슬롯(아케인 존)의 accumulatedDamage에 데미지 누적 후 즉시 저장
            GameDataManager.Instance.saveData.skillSaveList[1].accumulatedDamage += totalDamageThisFrame;
            GameDataManager.Instance.SaveJsonData();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!targetsInZone.Contains(collision))
            {
                targetsInZone.Add(collision);

                if (currentLevel >= 5)
                {
                    EnemyAI enemy = collision.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        // enemy.ApplySlow(slowMultiplier);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (targetsInZone.Contains(collision))
            {
                targetsInZone.Remove(collision);
            }

            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // enemy.RestoreSpeed(); // 장판에서 탈출했으므로 속도 원상복구
            }
        }
    }
}