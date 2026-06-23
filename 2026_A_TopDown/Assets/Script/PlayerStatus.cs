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

        // ★ [핵심 수정] 새 게임이 켜질 때, 세이브 데이터 및 필드의 모든 스킬 정보를 완벽하게 밀어버립니다.
        ResetAllSkillLevels();

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

        // 1. 몬스터 스포너의 난이도, 시간, 스폰량을 완전 초기화
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.ResetSpawner();
        }
        else
        {
            Debug.LogWarning("[EnemySpawner 경고] 씬에 EnemySpawner 오브젝트가 없거나 싱글톤이 비어있습니다.");
        }

        // 죽는 순간에도 안전하게 데이터 리셋 호출
        ResetAllSkillLevels();

        Debug.LogError("게임 오버! 플레이어가 사망했습니다. 모든 인게임 스킬 레벨이 리셋됩니다.");
        Time.timeScale = 0f; // 게임 일시정지

        // 2. LobbyManager에 있는 게임 오버 UI 팝업을 띄웁니다.
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.ShowGameOverUI();
        }
        else
        {
            Debug.LogWarning("[LobbyManager 경고] PlayScene 하이어라키 창에 LobbyManager 오브젝트가 배치되지 않았습니다!");
        }
    }

    /// <summary>
    /// ★ [수정완료] 외부 세이브 파일 데이터와 씬의 컴포넌트들을 일괄 격파하여 초기화하고 비주얼을 숨깁니다.
    /// </summary>
    private void ResetAllSkillLevels()
    {
        // 1. 레벨업 선택 메뉴를 통해 JSON 세이브 정보를 정비 (매직 애로우=1, 나머지=0)
        if (LevelUpMenu.Instance != null)
        {
            LevelUpMenu.Instance.ResetSaveDataSkills();
        }

        // 2. 씬에 살아 움직이는 실제 SkillData 컴포넌트들을 수집합니다.
        SkillData[] allSceneSkills = Object.FindObjectsByType<SkillData>(FindObjectsSortMode.None);

        if (allSceneSkills != null && allSceneSkills.Length > 0)
        {
            foreach (SkillData skill in allSceneSkills)
            {
                if (skill == null) continue;

                // 세이브 파일 데이터 원본 기준으로 씬 오브젝트 레벨 재동기화
                if (GameDataManager.Instance != null && GameDataManager.Instance.saveData != null)
                {
                    int skillIndex = (int)skill.skillType;
                    if (GameDataManager.Instance.saveData.skillSaveList.Count > skillIndex)
                    {
                        skill.currentLevel = GameDataManager.Instance.saveData.skillSaveList[skillIndex].level;
                    }
                }

                // ★ [비주얼 숨김 방어 코드] 레벨이 0이 된 아케인존 등 미해금 스킬은 오브젝트를 강제로 꺼서 이펙트를 숨깁니다.
                if (skill.currentLevel <= 0)
                {
                    skill.gameObject.SetActive(false);
                    Debug.Log($"<color=orange><b>[스킬 숨김]</b></color> {skill.skillType}이 0레벨이므로 오브젝트를 비활성화했습니다.");
                }
                else
                {
                    // 1레벨인 매직 애로우 등은 확실하게 활성화시킵니다.
                    skill.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogWarning("[스킬 리셋 경고] 현재 씬에서 SkillData 컴포넌트를 단 하나도 찾지 못했습니다.");
        }
    }
}