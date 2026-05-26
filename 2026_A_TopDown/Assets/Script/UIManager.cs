using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 필수 포함

public class UIManager : MonoBehaviour
{
    // 싱글톤 패턴을 적용해 어디서든 편하게 UI를 갱신할 수 있도록 합니다.
    public static UIManager instance;

    [Header("--- UI References ---")]
    [SerializeField] private Slider hpSlider;          // 체력 바 (Slider)
    [SerializeField] private TextMeshProUGUI hpText;   // 체력 텍스트 (TMP)
    [SerializeField] private TextMeshProUGUI killText; // 킬 카운트 텍스트 (TMP)

    private int killCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 플레이어의 현재 체력 UI를 업데이트하는 함수
    public void UpdateHPUI(float currentHp, float maxHp)
    {
        if (hpSlider != null)
        {
            // Slider의 value를 0~1 비율로 변환하여 반영
            hpSlider.value = currentHp / maxHp;
        }

        if (hpText != null)
        {
            // 예: "HP: 85 / 100" 형태로 표시
            hpText.text = $"HP: {Mathf.CeilToInt(currentHp)} / {maxHp}";
        }
    }

    // 몬스터를 잡았을 때 호출하여 킬 카운트를 올리는 함수
    public void AddKill()
    {
        killCount++;
        if (killText != null)
        {
            // 예: "KILLS: 0023" 형태로 4자리 고정 표시
            killText.text = $"KILLS: {killCount:D4}";
        }
    }
}