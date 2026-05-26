using System.Collections.Generic;
using UnityEngine;

public class InfinityMapManager : MonoBehaviour
{
    [Header("--- References ---")]
    public Transform player;          // 플레이어 트랜스폼 (필수)
    public GameObject chunkPrefab;    // [주의] 부모 Grid가 (0,0)에 오고 내부 Tilemap이 -8~8 좌표를 채운 프리팹이어야 합니다.

    [Header("--- Map Settings ---")]
    public int chunkSize = 32;         // 청크 한 변의 가로세로 타일 개수 (16개)
    public float tileSize = 0.16f;     // 타일 한 칸의 실제 기획 수치 (0.16)

    // 실제 청크 하나의 정확한 월드 크기 (16 * 0.16 = 2.56f)
    private float actualChunkSize;

    private Vector2Int currentChunkCoord; // 플레이어가 현재 위치한 청크 좌표
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        // 중요: 소수점 오차를 방지하기 위해 여기서 실제 크기를 미리 계산
        actualChunkSize = chunkSize * tileSize;

        // 플레이어의 초기 월드 위치를 청크 격자 좌표로 변환
        currentChunkCoord = GetChunkCoord(player.position);

        // 지연 없이 즉시 주변 맵 생성
        UpdateChunks();
    }

    void Update()
    {
        // 매 프레임 플레이어의 실시간 청크 좌표를 계산
        Vector2Int newChunkCoord = GetChunkCoord(player.position);

        // 플레이어가 현재 있는 청크 영역을 완전히 벗어나 다른 청크로 넘어갔을 때만 업데이트
        if (newChunkCoord != currentChunkCoord)
        {
            currentChunkCoord = newChunkCoord;
            UpdateChunks();
        }
    }

    // 월드 실제 좌표(World Position)를 청크 격자 좌표(Chunk Coordinate)로 변환하는 함수
    Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        // actualChunkSize(2.56f)를 사용하여 정확하게 칸수 계산
        int x = Mathf.FloorToInt(worldPos.x / actualChunkSize);
        int y = Mathf.FloorToInt(worldPos.y / actualChunkSize);
        return new Vector2Int(x, y);
    }

    // 청크들을 배치하고 멀어진 청크를 정리하는 핵심 함수
    void UpdateChunks()
    {
        // 이번 턴에 플레이어 주변에 무조건 활성화되어야 하는 3x3 청크 좌표 목록 (총 9개)
        List<Vector2Int> requiredChunks = new List<Vector2Int>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // 현재 플레이어 청크 좌표를 중심으로 주변 8칸 추가
                requiredChunks.Add(currentChunkCoord + new Vector2Int(x, y));
            }
        }

        // 1. 필요한 좌표에 청크가 없다면 새로 생성하여 배치
        foreach (Vector2Int coord in requiredChunks)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                // 정확한 월드 좌표 구하기: (X 격자번호 * 2.56f, Y 격자번호 * 2.56f, 0)
                Vector3 chunkWorldPos = new Vector3(coord.x * actualChunkSize, coord.y * actualChunkSize, 0);

                // [매우 중요] 청크 생성 (생성 후 부모 Grid 오브젝트의 Position이 이 좌표에 오게 됨)
                GameObject newChunk = Instantiate(chunkPrefab, chunkWorldPos, Quaternion.identity);

                // 계층 구조(Hierarchy) 창에서 알아보기 쉽게 이름 변경 (디버깅용)
                newChunk.name = $"Chunk_{coord.x}_{coord.y}";

                activeChunks.Add(coord, newChunk);
            }
        }

        // 2. 플레이어와 너무 멀어진 청크(3x3 영역 밖)는 리스트에서 제외하고 삭제
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            // 플레이어의 현재 청크 좌표와 거리가 가로 또는 세로로 1칸보다 멀다면 삭제 대상
            // (즉, 3x3 격자를 벗어난 좌표)
            if (Mathf.Abs(chunk.Key.x - currentChunkCoord.x) > 1 || Mathf.Abs(chunk.Key.y - currentChunkCoord.y) > 1)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        // 3. 삭제 대상 청크 메모리 해제
        foreach (Vector2Int coord in chunksToRemove)
        {
            Destroy(activeChunks[coord]);
            activeChunks.Remove(coord);
        }
    }
}