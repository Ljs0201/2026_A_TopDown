using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    // 다른 스크립트(EnemyAI 등)에서 나를 부를 수 있도록 만든 싱글톤 변수
    public static PlayerStatus Instance;

    [Header("--- Level & EXP (기획안 공식 적용) ---")]
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float maxExp;

    [Header("--- Player Stats (0.16 타일 최적화) ---")]
    public float moveSpeed = 1.2f;
    public float maxHp = 100f;
    public float currentHp;
    public float magnetRange = 0.64f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHp = maxHp;
        CalculateMaxExp();

        // 시작할 때 UI 매니저를 통해 HP 바와 EXP 바를 함께 초기화합니다.
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHPUI(currentHp, maxHp);
            UIManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);
        }
    }

    private void CalculateMaxExp()
    {
        maxExp = (currentLevel * 10f) + (Mathf.Pow(currentLevel, 2) * 1.5f);
        Debug.Log($"레벨 {currentLevel} 달성! 다음 레벨업까지 필요한 EXP: {maxExp}");
    }

    public void GainExp(float amount)
    {
        currentExp += amount;

        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }

        // [수정] 경험치를 먹을 때마다 실시간으로 UI 게이지와 숫자를 채워줍니다.
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        CalculateMaxExp();

        currentHp = maxHp; // 레벨업 시 체력 완전 회복

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHPUI(currentHp, maxHp);
            UIManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);
        }

        moveSpeed += 0.05f;
        magnetRange += 0.02f;

        Debug.LogWarning($"★ LEVEL UP! 현재 레벨: {currentLevel} ★");

        // ★ [연동 추가] 레벨업하는 순간 일시정지하며 랜덤 스킬 선택창을 화면에 오픈합니다!
        if (LevelUpMenu.Instance != null)
        {
            LevelUpMenu.Instance.ShowLevelUpMenu();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;

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
        Time.timeScale = 0f;
    }
}