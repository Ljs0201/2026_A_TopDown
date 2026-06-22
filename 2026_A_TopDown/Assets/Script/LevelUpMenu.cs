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
                    // 원소 구체(ElementalSphere) 타입 해금 및 이중 레벨업 버그 수정 완료
                    if (type == SkillData.SkillType.ElementalSphere)
                    {
                        ElementalSphere sphere = skill as ElementalSphere;
                        if (sphere != null)
                        {
                            sphere.isUnlocked = true;

                            if (sphere.currentLevel <= 5)
                            {
                                // ★ [버그 해결 핵심] 
                                // SyncLevelsToActiveSkills()에서 이미 타겟 레벨로 동기화가 끝났으므로,
                                // sphere.UnlockOrLevelUp() 내부의 레벨업(++ 연산)과 충돌하여 2레벨이 뛰는 걸 막기 위해
                                // 호출 직전에 임시로 레벨을 1 낮춰 상쇄시킵니다. 
                                // 이렇게 하면 구체 개수도 정확한 레벨에 맞춰 초기화되고, 최종 수치도 1만 깔끔하게 오릅니다.
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