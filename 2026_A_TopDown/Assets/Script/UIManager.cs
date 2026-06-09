using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("--- UI References ---")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI killText;

    // [수정] 상단 경험치 바와 텍스트를 제어하기 위한 새 변수들
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI levelText;

    private int killCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateHPUI(float currentHp, float maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHp / maxHp;
        }

        if (hpText != null)
        {
            hpText.text = $"HP: {Mathf.CeilToInt(currentHp)} / {maxHp}";
        }
    }

    // [수정] 경험치 바 및 레벨 수치를 받아와 화면을 실시간 업데이트하는 새 함수
    public void UpdateExpUI(float currentExp, float maxExp, int currentLevel)
    {
        if (expSlider != null)
        {
            expSlider.value = currentExp / maxExp;
        }

        if (expText != null)
        {
            expText.text = $"EXP: {Mathf.FloorToInt(currentExp)} / {Mathf.FloorToInt(maxExp)}";
        }

        if (levelText != null)
        {
            levelText.text = $"LV. {currentLevel}";
        }
    }

    public void AddKill()
    {
        killCount++;
        if (killText != null)
        {
            killText.text = $"KILLS: {killCount:D4}";
        }
    }
}