using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("--- Level & EXP (기획안 공식 적용) ---")]
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float maxExp; // 현재 레벨에서 요구하는 총 경험치

    [Header("--- Player Stats (0.16 타일 최적화) ---")]
    public float moveSpeed = 1.2f;      // 초당 1.2m 이동 (기획값)
    public float maxHp = 100f;
    public float currentHp;
    public float magnetRange = 0.64f;   // 자석 반경 (타일 4칸 거리)

    void Start()
    {
        currentHp = maxHp;
        CalculateMaxExp(); // 시작할 때 1레벨 요구 경험치 계산

        // [수정 반영] 게임 시작 시 우측 상단 체력 바 UI를 100% 가득 찬 상태로 초기화
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHPUI(currentHp, maxHp);
        }
    }

    // 기획서 공식: 요구 EXP = (현재 레벨 * 10) + (현재 레벨^2 * 1.5)
    private void CalculateMaxExp()
    {
        maxExp = (currentLevel * 10f) + (Mathf.Pow(currentLevel, 2) * 1.5f);
        Debug.Log($"레벨 {currentLevel} 달성! 다음 레벨업까지 필요한 EXP: {maxExp}");
    }

    // 외부(보석 루팅 등)에서 경험치를 획득할 때 호출할 함수
    public void GainExp(float amount)
    {
        currentExp += amount;

        // 경험치가 가득 차면 레벨업 처리 (연속 레벨업 대응)
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        CalculateMaxExp();

        // 레벨업 시 보상 (체력 완전 회복 및 스탯 미세 상승)
        currentHp = maxHp;

        // 레벨업 시 체력이 꽉 차므로 UI도 함께 맥스치로 갱신해줍니다.
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHPUI(currentHp, maxHp);
        }

        moveSpeed += 0.05f;
        magnetRange += 0.02f;

        // TODO: 여기에 추후 "레벨업 UI 선택 팝업창 띄우기" 기능을 추가할 예정입니다.
        Debug.LogWarning($"★ LEVEL UP! 현재 레벨: {currentLevel} ★");
    }

    // 몬스터에게 부딪혔을 때 대미지를 받는 함수
    public void TakeDamage(float damage)
    {
        currentHp -= damage;

        // [수정 반영] 대미지를 입을 때마다 실시간으로 우측 상단 UI 갱신
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHPUI(currentHp, maxHp);
        }

        if (currentHp <= 0)
        {
            currentHp = 0;
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.LogError("게임 오버! 플레이어가 사망했습니다.");
        Time.timeScale = 0f; // 게임 일시정지
    }
}