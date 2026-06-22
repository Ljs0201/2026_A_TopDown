using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
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

    private bool isDead = false; // ★ 중복 사망 및 사망 후 데미지 방지용 변수 추가

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ★ [영구 강화 스텟 반영] 기기에 저장된 레벨을 불러옵니다.
        int savedHpLevel = PlayerPrefs.GetInt("Stat_HP_Level", 0);
        int savedSpeedLevel = PlayerPrefs.GetInt("Stat_Speed_Level", 0);

        // 기본 스텟에 (레벨 * 가중치)를 더해줍니다.
        maxHp = 100f + (savedHpLevel * 20f);       // 1레벨당 최대 체력 +20 증가
        moveSpeed = 1.2f + (savedSpeedLevel * 0.1f); // 1레벨당 이동속도 +0.1 증가

        // 체력 초기화 및 UI 동기화
        currentHp = maxHp;
        CalculateMaxExp();

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
        if (isDead) return; // 이미 죽었다면 경험치 획득 불가

        currentExp += amount;

        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }

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

        if (LevelUpMenu.Instance != null)
        {
            LevelUpMenu.Instance.ShowLevelUpMenu();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // ★ 이미 죽은 상태라면 데미지를 받지 않음

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
        if (isDead) return;
        isDead = true; // 사망 확정 처리

        Debug.LogError("게임 오버! 플레이어가 사망했습니다.");
        Time.timeScale = 0f; // 게임 일시정지

        // ★ [핵심 추가] LobbyManager에 있는 게임 오버 UI 팝업을 띄웁니다!
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.ShowGameOverUI();
        }
        else
        {
            Debug.LogWarning("[LobbyManager 경고] PlayScene 하이어라키 창에 LobbyManager 오브젝트가 배치되지 않았습니다!");
        }
    }
}