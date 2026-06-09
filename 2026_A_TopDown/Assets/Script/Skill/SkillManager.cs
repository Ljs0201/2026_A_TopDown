using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("--- References ---")]
    public GameObject magicArrowPrefab; // 발사할 마법 화살 프리팹
    public Transform playerTransform;   // 플레이어 위치 정보
    public SkillData magicArrowData;    // 위에서 만든 스킬 데이터 연결

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerTransform == null) playerTransform = transform;

        // 정기적으로 매직 애로우 발사 루틴 시작
        StartCoroutine(MagicArrowRoutine());
    }

    IEnumerator MagicArrowRoutine()
    {
        while (true)
        {
            // 기획안에 설정된 쿨타임만큼 대기
            yield return new WaitForSeconds(magicArrowData.attackInterval);

            Transform target = FindClosestEnemy();

            // 필드에 적이 있을 때만 발사합니다.
            if (target != null)
            {
                FireMagicArrow(target);
            }
        }
    }

    // 맵 전체에서 가장 가까운 Enemy 태그의 몬스터를 찾는 함수
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

    // 레벨에 맞는 개수만큼 화살을 부채꼴 모양으로 전개하여 발사하는 함수
    private void FireMagicArrow(Transform target)
    {
        Vector3 fireDirection = (target.position - playerTransform.position).normalized;
        int level = magicArrowData.currentLevel;

        // 화살 여러 발 발사 시 퍼지는 각도 (레벨 1일 때는 0도 고정)
        float angleStep = 15f;
        float startAngle = -((level - 1) * angleStep) / 2f;

        for (int i = 0; i < level; i++)
        {
            float targetAngle = startAngle + (angleStep * i);

            // 발사 방향을 각도만큼 회전 연산
            Quaternion rotation = Quaternion.Euler(0, 0, targetAngle);
            Vector3 rotatedDir = rotation * fireDirection;

            // 화살 생성
            GameObject arrow = Instantiate(magicArrowPrefab, playerTransform.position, Quaternion.identity);

            // 화살 스크립트에 정보 주입 (방향, 속도, 대미지, 만렙 도탄 여부)
            MagicArrowProjectile projectile = arrow.GetComponent<MagicArrowProjectile>();
            if (projectile != null)
            {
                bool isLevel5 = (level == 5);
                projectile.Setup(rotatedDir, magicArrowData.arrowSpeed, magicArrowData.damage, isLevel5);
            }
        }
    }
}