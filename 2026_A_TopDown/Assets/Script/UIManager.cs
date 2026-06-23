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

    // ★ [새로 추가] 인스펙터 창에 노출시켜 줄 스테이지 & 타이머 UI 텍스트 변수
    [Header("--- 새로 추가할 Stage & Time UI ---")]
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI playTimeText;

    private int killCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // ★ [새로 추가] 매 프레임마다 스포너에서 시간을 실시간으로 훔쳐와 UI에 그려줍니다.
    private void Update()
    {
        UpdateStageAndTimeUI();
    }

    // ★ [새로 추가] 실시간 스테이지 및 시간 갱신 로직 함수
    private void UpdateStageAndTimeUI()
    {
        if (EnemySpawner.Instance != null)
        {
            // 1. 현재 스테이지(단계) 표시 (0단계부터 시작하므로 유저 시각에 맞춰 +1)
            if (stageText != null)
            {
                stageText.text = $"STAGE {EnemySpawner.Instance.currentStage + 1}";
            }

            // 2. 플레이 타임 표시 (초 단위를 분:초 형태로 포맷팅)
            if (playTimeText != null)
            {
                float time = EnemySpawner.Instance.playTime;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);

                // 00:00 형태로 출력
                playTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
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