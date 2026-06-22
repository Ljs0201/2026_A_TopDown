using UnityEngine;
using UnityEngine.UI; // 구버전 Text용
using TMPro; // ★ 신버전 TextMeshPro를 쓰기 위해 반드시 필요합니다!

public class CreditDisplayUI : MonoBehaviour
{
    [Header("--- UI 컴포넌트 연결 ---")]
    // [팁] 두 종류의 변수를 모두 열어두어 에디터에서 어떤 텍스트를 만들었든 다 들어갑니다!
    public Text legacyText;             // 일반 Text용 빈칸
    public TMP_Text textMeshProText;    // TextMeshPro (TMP)용 빈칸

    void Start()
    {
        UpdateCreditUI();
    }

    void Update()
    {
        UpdateCreditUI();
    }

    public void UpdateCreditUI()
    {
        // PlayerPrefs 저장소에서 현재 실시간 크레딧 잔액 땡겨오기
        int currentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);

        // 1. 만약 하이어라키에서 넣은게 구버전 일반 Text라면 여기에 글자 반영
        if (legacyText != null)
        {
            legacyText.text = $"보유 크레딧: {currentCredits}";
        }

        // 2. 만약 하이어라키에서 넣은게 신버전 TextMeshPro(TMP)라면 여기에 글자 반영
        if (textMeshProText != null)
        {
            textMeshProText.text = $"보유 크레딧: {currentCredits}";
        }
    }
}