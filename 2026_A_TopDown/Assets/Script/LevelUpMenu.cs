using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpMenu : MonoBehaviour
{
    public static LevelUpMenu Instance;

    // UI 레이아웃에 들어갈 스킬별 고유 정보 구조체
    [System.Serializable]
    public struct SkillUIData
    {
        public SkillData.SkillType type; // 유저님이 만든 SkillData의 enum 연동
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
        if (menuPanel != null) menuPanel.SetActive(false); // 시작할 땐 숨김
    }

    // PlayerStatus에서 호출하는 팝업 함수
    public void ShowLevelUpMenu()
    {
        Time.timeScale = 0f; // 게임 물리 일시정지
        menuPanel.SetActive(true);

        selectedIndices.Clear();
        List<int> availableIndices = new List<int>();

        // 10개 스킬 풀을 돌며 배울 수 있는(5레벨 미만) 스킬 필터링
        for (int i = 0; i < skillUIList.Count; i++)
        {
            SkillData matchSkill = activeSkills.Find(s => s.skillType == skillUIList[i].type);
            if (matchSkill == null || matchSkill.currentLevel < 5)
            {
                availableIndices.Add(i);
            }
        }

        // 후보군 중 랜덤으로 중복 없이 최대 3개 인덱스 선택
        int choicesCount = Mathf.Min(3, availableIndices.Count);
        while (selectedIndices.Count < choicesCount)
        {
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
            }
        }

        // 뽑힌 3개의 스킬 데이터를 실제 3개 UI 세트에 바인딩
        for (int i = 0; i < 3; i++)
        {
            if (i < selectedIndices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                int uiIndex = selectedIndices[i];
                SkillUIData uiData = skillUIList[uiIndex];

                // 현재 레벨을 조회하여 다음 레벨 수치를 UI에 표기
                SkillData matchSkill = activeSkills.Find(s => s.skillType == uiData.type);
                int nextLevel = (matchSkill != null) ? matchSkill.currentLevel + 1 : 1;

                nameTexts[i].text = $"{uiData.skillName} (LV.{nextLevel})";
                descTexts[i].text = uiData.description;
                if (iconImages[i] != null && uiData.skillIcon != null)
                {
                    iconImages[i].sprite = uiData.skillIcon;
                }

                // 버튼의 이전 이벤트 리스너를 비우고 새 함수 등록
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectSkill(uiData.type));
            }
            else
            {
                // 선택할 수 있는 스킬이 3개보다 적다면 남는 버튼 오브젝트 비활성화
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectSkill(SkillData.SkillType type)
    {
        SkillData matchSkill = activeSkills.Find(s => s.skillType == type);

        if (matchSkill != null)
        {
            // 인게임 실시간 스킬 레벨 1 증가
            matchSkill.currentLevel++;
            Debug.LogWarning($"{type} 스킬 레벨업 완료! 현재 레벨: {matchSkill.currentLevel}");
        }

        menuPanel.SetActive(false); // 선택창 닫기
        Time.timeScale = 1f;        // 게임 시간 정상 재개
    }
}