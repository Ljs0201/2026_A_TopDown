using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("--- References ---")]
    [SerializeField] private Transform player;        // 플레이어 위치 (기준점)
    [SerializeField] private GameObject enemyPrefab;  // 스폰할 몬스터 프리팹

    [Header("--- Spawn Settings ---")]
    [SerializeField] private float spawnInterval = 2.0f; // 스폰 주기 (2초)
    [SerializeField] private int spawnCount = 5;         // 한 번에 스폰할 마리 수

    [Header("--- Spawn Range (Camera Size 1.2 Optimized) ---")]
    // 카메라 Size 1.2 (대각선 끝 2.44m) 화면에서 안 보이게 생성되는 최적의 수치입니다.
    [SerializeField] private float minSpawnRadius = 2.6f;
    [SerializeField] private float maxSpawnRadius = 4.5f;

    [Header("--- Optimization ---")]
    [SerializeField] private int maxEnemyCount = 800;    // 필드 최대 몬스터 수 한계치

    // 현재 필드에 살아있는 몬스터들을 추적하기 위한 리스트
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        // 지정된 주기(spawnInterval)마다 정기적으로 스폰 루틴 실행
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        // 실시간 디버깅용: 씬 창에서 범위를 계속 확인하고 싶다면 컴포넌트가 활성화된 상태를 유지합니다.
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

        for (int i = 0; i < spawnCount; i++)
        {
            // 플레이어 주변의 무작위 '시야 밖 도넛 영역' 좌표 구하기
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // 몬스터 생성
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // 추적 리스트에 추가
            spawnedEnemies.Add(enemy);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // 1. 무작위 방향 벡터 구하기 (원형의 360도 방향 벡터)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // 2. 최소 거리(2.6)와 최대 거리(4.5) 사이의 무작위 반경(거리) 구하기
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);

        // 3. 플레이어 현재 위치에 방향과 거리를 더해 최종 월드 좌표 연산
        Vector3 spawnPos = player.position + new Vector3(randomDirection.x * randomRadius, randomDirection.y * randomRadius, 0);

        return spawnPos;
    }

    // 에디터 씬(Scene) 창에서 스폰 영역을 시각적으로 확인하기 위한 기즈모 코드
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // 최소 스폰 반경은 빨간색 원으로 표시 (화면 안에서 몬스터가 툭 떨어지지 않는 한계선)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minSpawnRadius);

        // 최대 스폰 반경은 파란색 원으로 표시 (너무 멀지 않게 따라올 수 있는 외곽선)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, maxSpawnRadius);
    }
}