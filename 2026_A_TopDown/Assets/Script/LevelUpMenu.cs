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

        // ★ [핵심 추가] 고를 수 있는 스킬이 0개라면 (모든 스킬 만렙 상황)
        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("모든 스킬이 이미 최고 레벨입니다. 스킬 선택창을 띄우지 않고 게임을 진행합니다.");
            menuPanel.SetActive(false);
            Time.timeScale = 1f; // 게임 정지 없이 즉시 해제 및 재생
            return;
        }

        // 선택창이 정상적으로 뜰 때만 게임을 일시정지하고 패널을 활성화합니다.
        Time.timeScale = 0f;
        menuPanel.SetActive(true);

        // 랜덤 3개 추출
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

            SyncLevelsToActiveSkills();
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