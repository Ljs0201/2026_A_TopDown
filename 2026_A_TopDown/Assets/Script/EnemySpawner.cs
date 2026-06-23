using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance; // ★ 리셋 연동을 위한 싱글톤 추가

    [Header("--- References ---")]
    [SerializeField] private Transform player;        // 플레이어 위치 (기준점)
    [SerializeField] private GameObject enemyPrefab;  // 스폰할 몬스터 프리팹

    [Header("--- Spawn Settings ---")]
    [SerializeField] private float spawnInterval = 2.0f; // 스폰 주기 (2초)
    [SerializeField] private int baseSpawnCount = 5;     // ★ 기본 한 번에 스폰할 마릿수

    [Header("--- Spawn Range (Camera Size 1.2 Optimized) ---")]
    [SerializeField] private float minSpawnRadius = 2.6f;
    [SerializeField] private float maxSpawnRadius = 4.5f;

    [Header("--- Optimization ---")]
    [SerializeField] private int maxEnemyCount = 800;    // 필드 최대 몬스터 수 한계치

    [Header("--- ★ 시간 비례 난이도 세팅 ★ ---")]
    [SerializeField] private float difficultyIncreaseInterval = 10f; // 몇 초마다 강해질지 (예: 10초)
    [SerializeField] private int countIncreasePerStage = 1;          // 단계별 추가 스폰 마릿수 (+1)
    [SerializeField] private float hpIncreasePerStage = 10f;         // 단계별 몬스터 추가 체력 (+10)
    [SerializeField] private float damageIncreasePerStage = 2f;      // 단계별 몬스터 추가 공격력 (+2)

    [Header("--- 실시간 데이터 (디버깅용) ---")]
    public float playTime = 0f;
    public int currentStage = 0;

    // 현재 필드에 살아있는 몬스터들을 추적하기 위한 리스트
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;

    void Awake()
    {
        Instance = this; // 싱글톤 지정
    }

    void Start()
    {
        ResetSpawner(); // 첫 시작할 때 스포너를 원상복구 상태로 실행
    }

    void Update()
    {
        // 1. 플레이 타임 실시간 누적
        playTime += Time.deltaTime;

        // 2. 시간에 비례해 난이도 단계(Stage) 계산
        int newStage = Mathf.FloorToInt(playTime / difficultyIncreaseInterval);
        if (newStage != currentStage)
        {
            currentStage = newStage;
            Debug.LogWarning($"<color=red><b>[난이도 단계 상승!]</b></color> 현재 {currentStage}단계 / 스폰량: {GetBonusSpawnCount()}마리 / 보너스 HP: {GetBonusHP()}, 보너스 ATK: {GetBonusDamage()}");
        }
    }

    // 현재 단계의 최종 스폰 마릿수 계산
    private int GetBonusSpawnCount()
    {
        return baseSpawnCount + (currentStage * countIncreasePerStage);
    }

    // 현재 단계의 보너스 HP
    public float GetBonusHP()
    {
        return currentStage * hpIncreasePerStage;
    }

    // 현재 단계의 보너스 공격력
    public float GetBonusDamage()
    {
        return currentStage * damageIncreasePerStage;
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 리스트에서 이미 죽어서 파괴된(null) 몬스터들을 먼저 정리합니다.
            spawnedEnemies.RemoveAll(item => item == null);

            // 필드에 몬스터가 최대 제한치보다 적을 때만 새로 스폰합니다.
            if (spawnedEnemies.Count < maxEnemyCount)
            {
                SpawnEnemies();
            }
        }
    }

    private void SpawnEnemies()
    {
        if (player == null || enemyPrefab == null) return;

        // ★ 실시간 난이도가 적용된 마릿수만큼 소환
        int targetSpawnCount = GetBonusSpawnCount();

        for (int i = 0; i < targetSpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // ★ [핵심] 생성된 몬스터에게 시간에 비례한 보너스 수치 주입!
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.SetupBonusStats(GetBonusHP(), GetBonusDamage());
            }

            spawnedEnemies.Add(enemy);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPos = player.position + new Vector3(randomDirection.x * randomRadius, randomDirection.y * randomRadius, 0);
        return spawnPos;
    }

    /// <summary>
    /// ★ 플레이어 사망 시 외부에서 호출해 줄 리셋 치트키 함수
    /// </summary>
    public void ResetSpawner()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        playTime = 0f;
        currentStage = 0;

        spawnCoroutine = StartCoroutine(SpawnRoutine());
        Debug.Log("<color=cyan><b>[스포너 리셋 완료]</b></color> 몬스터 스폰량 및 추가 스텟 난이도가 초기화되었습니다.");
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minSpawnRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, maxSpawnRadius);
    }
}