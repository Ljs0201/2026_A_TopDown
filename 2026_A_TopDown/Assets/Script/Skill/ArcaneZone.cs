using UnityEngine;
using System.Collections.Generic;

public class ArcaneZone : SkillData
{
    [Header("--- 아케인 존 밸런스 세팅 ---")]
    public float baseDamage = 5f;
    public float damagePerLevel = 3f;
    public float damageInterval = 0.5f;


    private float timer = 0f;
    private List<Collider2D> targetsInZone = new List<Collider2D>();

    // 이펙트 프리팹을 담아두는 자식 오브젝트를 통째로 제어합니다.
    private GameObject visualEffectObject;
    private CircleCollider2D circleCollider;
    private bool isEffectActivated = false; // 이펙트가 한 번 켜졌는지 체크용

    void Awake()
    {
        skillType = SkillType.ArcaneZone;
        circleCollider = GetComponent<CircleCollider2D>();

        // 내 자식으로 들어가 있는 에셋 프리팹 오브젝트를 찾습니다.
        if (transform.childCount > 0)
        {
            visualEffectObject = transform.GetChild(0).gameObject;
        }
    }

    void Start()
    {
        // 게임 첫 시작 시 레벨이 0이면 자식 이펙트 오브젝트와 충돌창을 완전히 꺼버립니다.
        if (currentLevel <= 0)
        {
            if (visualEffectObject != null) visualEffectObject.SetActive(false);
            if (circleCollider != null) circleCollider.enabled = false;
            isEffectActivated = false;
        }
    }

    void Update()
    {
        // 0레벨(잠금)일 때는 아무것도 하지 않고 패스합니다.
        if (currentLevel <= 0)
        {
            return;
        }

        // ★ [핵심 수정] 1레벨 이상이 되었을 때, 딱 한 번만 오브젝트를 켜서 이펙트를 깨웁니다!
        if (!isEffectActivated)
        {
            isEffectActivated = true;
            if (visualEffectObject != null) visualEffectObject.SetActive(true);
            if (circleCollider != null) circleCollider.enabled = true;
        }

        // [기획안 공식 반영] 1cell = 0.5f 기준 / 레벨업 시 1cell씩 크기 증가
        float cellCount = 3f + (currentLevel - 1) * 1.0f;
        float finalScale = cellCount * 0.5f;

        // Z축까지 3D 스케일 균등 조정하여 파티클 왜곡을 원천 차단합니다.
        transform.localScale = new Vector3(finalScale, finalScale, finalScale);

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
        float totalDamageThisFrame = 0f;

        for (int i = targetsInZone.Count - 1; i >= 0; i--)
        {
            if (targetsInZone[i] == null || !targetsInZone[i].gameObject.activeSelf)
            {
                targetsInZone.RemoveAt(i);
                continue;
            }

            EnemyAI enemy = targetsInZone[i].GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                totalDamageThisFrame += currentDamage;
            }
        }

        if (totalDamageThisFrame > 0f && GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 1)
        {
            GameDataManager.Instance.saveData.skillSaveList[1].accumulatedDamage += totalDamageThisFrame;
            GameDataManager.Instance.SaveJsonData();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!targetsInZone.Contains(collision)) targetsInZone.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (targetsInZone.Contains(collision)) targetsInZone.Remove(collision);
        }
    }
}