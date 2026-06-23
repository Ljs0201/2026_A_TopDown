using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpMenu : MonoBehaviour
{
    public static LevelUpMenu Instance;

    [System.Serializable]
    public struct SkillUIData
    {
        public SkillData.SkillType type;
        public string skillName;
        [TextArea] public string description;
        public Sprite skillIcon;
    }

    [Header("--- 10개 스킬 정보 설정 풀(Pool) ---")]
    public List<SkillUIData> skillUIList = new List<SkillUIData>();

    [Header("--- 인게임에 배치된 실제 스킬 오브젝트들 ---")]
    public List<SkillData> activeSkills = new List<SkillData>();

    [Header("--- UI 컴포넌트 연결 창 ---")]
    public GameObject menuPanel;
    public Button[] choiceButtons = new Button[3];
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] descTexts = new TextMeshProUGUI[3];
    public Image[] iconImages = new Image[3];

    private List<int> selectedIndices = new List<int>();

    void Awake()
    {
        Instance = this;
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void Start()
    {
        SyncLevelsToActiveSkills();
    }

    // ★ [수정완료] 매직 애로우는 1레벨, 나머지는 0레벨로 JSON 세이브 데이터를 초기화합니다.
    public void ResetSaveDataSkills()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.saveData != null && GameDataManager.Instance.saveData.skillSaveList != null)
        {
            for (int i = 0; i < GameDataManager.Instance.saveData.skillSaveList.Count; i++)
            {
                SkillData.SkillType currentType = (SkillData.SkillType)i;

                // 매직 애로우(MagicArrow)만 1레벨 시작, 나머지는 0레벨(미해금)로 세팅
                if (currentType == SkillData.SkillType.MagicArrow)
                {
                    GameDataManager.Instance.saveData.skillSaveList[i].level = 1;
                }
                else
                {
                    GameDataManager.Instance.saveData.skillSaveList[i].level = 0;
                }
            }

            // 변경된 상태를 JSON 파일로 저장
            GameDataManager.Instance.SaveJsonData();

            // 실제 월드에 배치된 스킬 컴포넌트들의 레벨도 동기화
            SyncLevelsToActiveSkills();

            Debug.Log("<color=cyan><b>[JSON 데이터 초기화]</b></color> 매직 애로우=LV.1 / 나머지 스킬=LV.0 초기화 완료.");
        }
    }

    public void ShowLevelUpMenu()
    {
        selectedIndices.Clear();
        List<int> availableIndices = new List<int>();

        int maxLvl = (GameDataManager.Instance != null && GameDataManager.Instance.gameSettingData != null)
            ? GameDataManager.Instance.gameSettingData.maxSkillLevel : 5;

        // 1. 만렙이 아닌 스킬 후보군 수집
        for (int i = 0; i < skillUIList.Count; i++)
        {
            int skillIndex = (int)skillUIList[i].type;
            int currentLvl = 0;

            if (GameDataManager.Instance != null && GameDataManager.Instance.saveData != null && GameDataManager.Instance.saveData.skillSaveList.Count > skillIndex)
            {
                currentLvl = GameDataManager.Instance.saveData.skillSaveList[skillIndex].level;
            }

            if (currentLvl < maxLvl)
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("모든 스킬이 이미 최고 레벨입니다. 스킬 선택창을 띄우지 않고 게임을 진행합니다.");
            menuPanel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        Time.timeScale = 0f;
        menuPanel.SetActive(true);

        int choicesCount = Mathf.Min(3, availableIndices.Count);
        while (selectedIndices.Count < choicesCount)
        {
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            if (!selectedIndices.Contains(randomIndex)) selectedIndices.Add(randomIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            if (i < selectedIndices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                int uiIndex = selectedIndices[i];
                SkillUIData uiData = skillUIList[uiIndex];

                int skillIndex = (int)uiData.type;
                int currentLvl = 0;
                if (GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > skillIndex)
                {
                    currentLvl = GameDataManager.Instance.saveData.skillSaveList[skillIndex].level;
                }
                int nextLevel = currentLvl + 1;

                nameTexts[i].text = uiData.skillName + " (LV." + nextLevel + ")";
                descTexts[i].text = uiData.description;

                if (iconImages[i] != null && uiData.skillIcon != null)
                {
                    iconImages[i].sprite = uiData.skillIcon;
                }

                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectSkill(uiData.type));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectSkill(SkillData.SkillType type)
    {
        int skillIndex = (int)type;

        if (GameDataManager.Instance != null && GameDataManager.Instance.saveData != null)
        {
            if (GameDataManager.Instance.saveData.skillSaveList.Count > skillIndex)
            {
                GameDataManager.Instance.saveData.skillSaveList[skillIndex].level++;
            }

            // 1. 단순 레벨 수치 동기화 실행
            SyncLevelsToActiveSkills();

            // 2. 인게임에 배치된 스킬 중, 방금 선택한 타입의 실체 오브젝트를 찾아서 특수 로직 가동
            foreach (SkillData skill in activeSkills)
            {
                if (skill != null && skill.skillType == type)
                {
                    // ★ [핵심 추가] 선택해서 레벨이 1 이상이 된 스킬 오브젝트는 비주얼(이펙트)을 위해 즉시 활성화합니다.
                    if (skill.currentLevel > 0 && !skill.gameObject.activeSelf)
                    {
                        skill.gameObject.SetActive(true);
                        Debug.Log($"<color=lime><b>[스킬 해금 및 활성화]</b></color> {skill.skillType} 오브젝트가 켜졌습니다.");
                    }

                    // 원소 구체(ElementalSphere) 타입 해금 및 이중 레벨업 버그 수정 완료
                    if (type == SkillData.SkillType.ElementalSphere)
                    {
                        ElementalSphere sphere = skill as ElementalSphere;
                        if (sphere != null)
                        {
                            sphere.isUnlocked = true;

                            if (sphere.currentLevel <= 5)
                            {
                                sphere.currentLevel--;
                                sphere.UnlockOrLevelUp();
                            }
                        }
                    }
                    // 낙뢰(LightningStrike) 타입 잠금 해제 연동
                    else if (type == SkillData.SkillType.LightningStrike)
                    {
                        LightningStrike lightning = skill as LightningStrike;
                        if (lightning != null) lightning.isUnlocked = true;
                    }
                    // 아케인존(ArcaneZone) 또는 기타 스킬 추가 확장 필요 시 여기에 추가 가능
                }
            }

            GameDataManager.Instance.SaveJsonData();
        }

        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void SyncLevelsToActiveSkills()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.saveData == null) return;

        foreach (SkillData skill in activeSkills)
        {
            if (skill == null) continue;
            int skillIndex = (int)skill.skillType;

            if (GameDataManager.Instance.saveData.skillSaveList.Count > skillIndex)
            {
                skill.currentLevel = GameDataManager.Instance.saveData.skillSaveList[skillIndex].level;
                Debug.Log($"[{skill.skillType}] 이름 매칭 동기화 완료 ➡️ LV.{skill.currentLevel}");
            }
        }
    }
}