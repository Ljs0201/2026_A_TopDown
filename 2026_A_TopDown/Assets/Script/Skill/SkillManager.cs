using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("--- 플레이어 정보 ---")]
    public Transform playerTransform;   // 플레이어 위치 정보

    [Header("--- 1. 매직 애로우 (MagicArrow) ---")]
    public GameObject magicArrowPrefab;
    public SkillData magicArrowData;

    [Header("--- 2. 낙뢰 (LightningStrike) ---")]
    public GameObject lightningStrikePrefab; // 낙뢰 이펙트/프리팹 넣는 칸
    public SkillData lightningStrikeData;    // 낙뢰 스킬데이터 컴포넌트 넣는 칸

    [Header("--- 3. 아케인존 (ArcaneZone) ---")]
    public GameObject arcaneZonePrefab;      // 아케인존 프리팹 넣는 칸
    public SkillData arcaneZoneData;         // 아케인존 스킬데이터 컴포넌트 넣는 칸

    [Header("--- 4. 원소 구체 (ElementalSphere) ---")]
    public GameObject elementalSpherePrefab; // 원소 구체 프리팹 넣는 칸
    public SkillData elementalSphereData;    // 원소 구체 스킬데이터 컴포넌트 넣는 칸

    [Header("--- ★ 통합 스킬 오디오 앰프 ★ ---")]
    public AudioSource audioSource;     // 소리를 뿜어줄 오디오 소스 컴포넌트

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerTransform == null) playerTransform = transform;

        // 정기적으로 각 스킬들이 발사되는 루틴들을 켭니다.
        StartCoroutine(MagicArrowRoutine());
        // StartCoroutine(LightningStrikeRoutine()); // 낙뢰 루틴이 있다면 주석 해제 하세요!
        // StartCoroutine(ArcaneZoneRoutine());
        // StartCoroutine(ElementalSphereRoutine());
    }

    IEnumerator MagicArrowRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(magicArrowData.attackInterval);
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                FireMagicArrow(target);
            }
        }
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = playerTransform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, currentPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
        return closestEnemy;
    }

    /// <summary>
    ///어떤 스킬이든 SkillData 컴포넌트만 넘겨주면 고유 소리를 재생해 주는 함수
    /// </summary>
    public void PlaySkillSound(SkillData data)
    {
        if (audioSource != null && data != null && data.skillSound != null)
        {
            audioSource.PlayOneShot(data.skillSound);
        }
    }

    // 1. 매직 애로우 발사
    private void FireMagicArrow(Transform target)
    {
        Vector3 fireDirection = (target.position - playerTransform.position).normalized;
        int level = magicArrowData.currentLevel;

        PlaySkillSound(magicArrowData); // 효과음 뿜!

        float angleStep = 15f;
        float startAngle = -((level - 1) * angleStep) / 2f;

        for (int i = 0; i < level; i++)
        {
            float targetAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, targetAngle);
            Vector3 rotatedDir = rotation * fireDirection;

            GameObject arrow = Instantiate(magicArrowPrefab, playerTransform.position, Quaternion.identity);
            MagicArrowProjectile projectile = arrow.GetComponent<MagicArrowProjectile>();
            if (projectile != null)
            {
                bool isLevel5 = (level == 5);
                projectile.Setup(rotatedDir, magicArrowData.arrowSpeed, magicArrowData.damage, isLevel5);
            }
        }
    }

    // 2. 낙뢰 발사 함수 예시
    public void FireLightningStrike()
    {
        PlaySkillSound(lightningStrikeData); // 낙뢰 소리 뿜!
        // ... 낙뢰 소환 로직 ...
    }

    // 3. 아케인존 발사 함수 예시
    public void FireArcaneZone()
    {
        PlaySkillSound(arcaneZoneData); // 아케인존 소리 뿜!
        // ... 아케인존 소환 로직 ...
    }

    // 4. 원소 구체 발사 함수 예시
    public void FireElementalSphere()
    {
        PlaySkillSound(elementalSphereData); // 원소 구체 소리 뿜!
        // ... 원소 구체 소환 로직 ...
    }
}