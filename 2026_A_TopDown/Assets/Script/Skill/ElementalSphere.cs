using UnityEngine;
using System.Collections.Generic;

public class ElementalSphere : SkillData
{
    [Header("--- 원소 구체 밸런스 세팅 ---")]
    public float baseDamage = 10f;
    public float damagePerLevel = 4f;
    public float orbitRadius = 2.5f;       // 플레이어와 구체 사이의 거리 (중거리)
    public float orbitSpeed = 150f;        // 회전 속도 (도/초)

    [Header("--- 5레벨(마스터) 특수 효과 ---")]
    public float explosionRadius = 2.0f;   // 1% 확률 폭발 범위
    public float explosionDamage = 40f;    // 폭발 광역 데미지
    public GameObject explosionEffectPrefab; // 폭발 파티클 프리팹

    [Header("--- 프리팹 및 해금 세팅 ---")]
    public GameObject spherePrefab;        // ★ 오류가 났던 변수 선언 완료!
    public bool isUnlocked = false;

    private Transform playerTransform;
    private List<GameObject> activeSpheres = new List<GameObject>();
    private float currentAngle = 0f;

    void Awake()
    {
        // SkillType 열거형에 ElementalSphere가 선언되어 있어야 합니다.
        skillType = SkillType.ElementalSphere;
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 게임 시작 시 기존에 세이브된 레벨 정보가 있다면 불러와 구체 복구
        if (GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 3)
        {
            int savedLevel = GameDataManager.Instance.saveData.skillSaveList[3].level;

            if (savedLevel > 0)
            {
                isUnlocked = true;
                currentLevel = savedLevel;

                // 저장된 레벨에 맞춰 구체 개수 복구 (최대 4랩 기준으로 2개씩 생산)
                int spawnCount = Mathf.Min(currentLevel, 4) * 2;
                SpawnSpheres(spawnCount);
            }
        }
    }

    void Update()
    {
        // 잠겨있거나 레벨이 0이거나 플레이어가 없으면 공전 연산 패스
        if (!isUnlocked || currentLevel <= 0 || playerTransform == null)
        {
            return;
        }

        // 마스터 오브젝트 위치를 플레이어에게 항상 동기화
        transform.position = playerTransform.position;

        // 회전 각도 누적 연산
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        // 자식 구체들 원형 배치 규칙 적용
        ArrangeSpheres();
    }

    // 레벨업 UI에서 스킬을 골랐을 때 호출하는 함수
    public void UnlockOrLevelUp()
    {
        if (!isUnlocked)
        {
            isUnlocked = true;
            currentLevel = 1;
            SpawnSpheres(2); // 1랩: 2개 생성
            Debug.Log("원소 구체 해금! (Lv.1)");
        }
        else
        {
            if (currentLevel < 4)
            {
                currentLevel++;
                SpawnSpheres(2); // 레벨당 2개씩 추가 생성 (최대 8개)
                Debug.Log($"원소 구체 레벨업! (Lv.{currentLevel})");
            }
            else if (currentLevel == 4)
            {
                // 4랩(만랩) 상태에서 추가 선택 시 5레벨(1% 폭발 특성) 해금
                currentLevel = 5;
                Debug.Log("원소 구체 5레벨 달성! 이제 구체가 적을 때릴 때 1% 확률로 광역 피해를 줍니다.");
            }
        }

        // 변경된 레벨 정보를 JSON 세이브 데이터에 즉시 저장
        if (GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 3)
        {
            GameDataManager.Instance.saveData.skillSaveList[3].level = currentLevel;
            GameDataManager.Instance.SaveJsonData();
        }
    }

    private void SpawnSpheres(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // spherePrefab을 내 자식 오브젝트로 생성
            GameObject sphere = Instantiate(spherePrefab, transform);

            // 생성된 구체에 부모 정보(this) 주입
            OrbitSphereChild sphereScript = sphere.GetComponent<OrbitSphereChild>();
            if (sphereScript != null) sphereScript.Setup(this);

            activeSpheres.Add(sphere);
        }
    }

    private void ArrangeSpheres()
    {
        int total = activeSpheres.Count;
        if (total == 0) return;

        float angleStep = 360f / total;

        for (int i = 0; i < total; i++)
        {
            if (activeSpheres[i] == null) continue;

            float sphereAngle = currentAngle + (i * angleStep);
            float rad = sphereAngle * Mathf.Deg2Rad;

            // 중거리 반지름(orbitRadius) 공식 적용
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
            activeSpheres[i].transform.position = transform.position + offset;
        }
    }

    public float GetCurrentDamage()
    {
        return baseDamage + (currentLevel - 1) * damagePerLevel;
    }
}