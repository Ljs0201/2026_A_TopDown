using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("--- 스텟별 현재 레벨 (기기 저장용) ---")]
    public int hpLevel = 0;
    public int speedLevel = 0;
    public int damageLevel = 0;

    [Header("--- 강화 비용 공식 세팅 (50% 복리) ---")]
    [SerializeField] private int baseUpgradeCost = 10;     // 시작 비용 (0레벨일 때)
    [SerializeField] private float costMultiplier = 1.5f;  // ★ 배율 (50% 상승이므로 1.5배)

    [Header("--- 로비 UI 텍스트 연결 ---")]
    public TMP_Text hpText;
    public TMP_Text speedText;
    public TMP_Text damageText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 게임이 켜지면 저장되어 있던 스텟 레벨들을 불러옵니다.
        hpLevel = PlayerPrefs.GetInt("Stat_HP_Level", 0);
        speedLevel = PlayerPrefs.GetInt("Stat_Speed_Level", 0);
        damageLevel = PlayerPrefs.GetInt("Stat_Damage_Level", 0);

        // 텍스트 새로고침
        UpdateUpgradeUI();
    }

    /// <summary>
    /// ★ [수정] 복리 계산 공식: baseCost * (1.5 ^ 현재레벨)
    /// </summary>
    private int GetCost(int currentStatLevel)
    {
        // 거듭제곱 함수(Mathf.Pow)를 이용해 레벨이 오를 때마다 1.5배씩 곱해지도록 합니다.
        float calculatedCost = baseUpgradeCost * Mathf.Pow(costMultiplier, currentStatLevel);

        // 소수점 이하 자리는 깔끔하게 반올림하여 정수로 반환합니다.
        return Mathf.RoundToInt(calculatedCost);
    }

    // 1. 체력 강화 버튼에 연결할 함수
    public void UpgradeHP()
    {
        int currentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);
        int requiredCost = GetCost(hpLevel);

        if (currentCredits >= requiredCost)
        {
            currentCredits -= requiredCost;
            PlayerPrefs.SetInt("PermanentCredits", currentCredits);

            hpLevel++;
            PlayerPrefs.SetInt("Stat_HP_Level", hpLevel);
            PlayerPrefs.Save();

            Debug.Log($"[강화 성공] 체력 레벨 업! 현재 레벨: {hpLevel} | 다음 비용: {GetCost(hpLevel)}");

            if (LobbyManager.Instance != null) LobbyManager.Instance.UpdateLobbyCreditUI();
            UpdateUpgradeUI();
        }
        else
        {
            Debug.LogWarning("크레딧이 부족합니다!");
        }
    }

    // 2. 이동속도 강화 버튼에 연결할 함수
    public void UpgradeSpeed()
    {
        int currentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);
        int requiredCost = GetCost(speedLevel);

        if (currentCredits >= requiredCost)
        {
            currentCredits -= requiredCost;
            PlayerPrefs.SetInt("PermanentCredits", currentCredits);

            speedLevel++;
            PlayerPrefs.SetInt("Stat_Speed_Level", speedLevel);
            PlayerPrefs.Save();

            Debug.Log($"[강화 성공] 이동속도 레벨 업! 현재 레벨: {speedLevel} | 다음 비용: {GetCost(speedLevel)}");

            if (LobbyManager.Instance != null) LobbyManager.Instance.UpdateLobbyCreditUI();
            UpdateUpgradeUI();
        }
        else
        {
            Debug.LogWarning("크레딧이 부족합니다!");
        }
    }

    // 3. 공격력 강화 버튼에 연결할 함수
    public void UpgradeDamage()
    {
        int currentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);
        int requiredCost = GetCost(damageLevel);

        if (currentCredits >= requiredCost)
        {
            currentCredits -= requiredCost;
            PlayerPrefs.SetInt("PermanentCredits", currentCredits);

            damageLevel++;
            PlayerPrefs.SetInt("Stat_Damage_Level", damageLevel);
            PlayerPrefs.Save();

            Debug.Log($"[강화 성공] 공격력 레벨 업! 현재 레벨: {damageLevel} | 다음 비용: {GetCost(damageLevel)}");

            if (LobbyManager.Instance != null) LobbyManager.Instance.UpdateLobbyCreditUI();
            UpdateUpgradeUI();
        }
        else
        {
            Debug.LogWarning("크레딧이 부족합니다!");
        }
    }

    /// <summary>
    /// 레벨과 유동적인 비용을 UI 텍스트에 뿌려주는 함수
    /// </summary>
    public void UpdateUpgradeUI()
    {
        if (hpText != null)
            hpText.text = $"체력 증가\n<size=80%>LV.{hpLevel}</size> <color=#FFCC00>({GetCost(hpLevel)} 크레딧)</color>";

        if (speedText != null)
            speedText.text = $"이동속도 증가\n<size=80%>LV.{speedLevel}</size> <color=#FFCC00>({GetCost(speedLevel)} 크레딧)</color>";

        if (damageText != null)
            damageText.text = $"공격력 증가\n<size=80%>LV.{damageLevel}</size> <color=#FFCC00>({GetCost(damageLevel)} 크레딧)</color>";
    }
}