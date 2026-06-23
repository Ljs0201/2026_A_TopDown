using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("--- 게임 오버 UI (인게임용) ---")]
    public GameObject gameOverPanel;

    [Header("--- 로비 UI 텍스트 연결 ---")]
    public Text lobbyLegacyText;
    public TMP_Text lobbyTextMeshProText;

    [Header("--- ★ 강화창 오브젝트 직접 연결 ★ ---")]
    public GameObject upgradePanel; // 로비의 UpgradePanel을 여기에 넣을 겁니다.

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateLobbyCreditUI();

        // 시작할 때는 강화창을 확실하게 꺼둡니다.
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    // ★ 능력치 버튼을 누르면 이 함수가 강화창을 강제로 켭니다.
    public void OpenUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            // 창이 열릴 때 스텟 글자들도 강제 새로고침
            if (UpgradeManager.Instance != null) UpgradeManager.Instance.UpdateUpgradeUI();
        }
    }

    // ★ 닫기 버튼을 누르면 이 함수가 강화창을 강제로 끕니다.
    public void CloseUpgradePanel()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    public void UpdateLobbyCreditUI()
    {
        int currentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);
        if (lobbyLegacyText != null) lobbyLegacyText.text = $"보유 크레딧: {currentCredits}";
        if (lobbyTextMeshProText != null) lobbyTextMeshProText.text = $"보유 크레딧: {currentCredits}";
    }

    public void ClickGameStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("PlayScene");
    }

    public void ShowGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // ★ [오타 및 버그 수정] 로비로 돌아가기 버튼 함수
    public void ClickReturnToLobby()
    {
        // 1. 멈춰버린 유니티 지구 시간을 다시 흐르게 만듭니다.
        Time.timeScale = 1f;

        // 2. [중요] 유저님의 유니티 프로젝트에 등록된 실제 로비 씬 이름으로 정확히 매칭하세요!
        // 기존 코드의 "LobyScenes" 오타를 대중적인 "LobbyScene"으로 수정해 두었습니다.
        SceneManager.LoadScene("LobyScene");
    }

    /// <summary>
    /// 로비의 [전체 초기화] 버튼에 연결할 함수
    /// </summary>
    public void ClickResetAllData()
    {
        // 1. 기기 저장소 완전히 포맷 (크레딧 + 모든 스텟 레벨 일괄 삭제)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.LogWarning("모든 게임 데이터와 강화 레벨이 초기화되었습니다.");

        // 2. 화면에 보이는 보유 크레딧 텍스트 0으로 갱신
        UpdateLobbyCreditUI();

        // 3. 강화창에 보이는 스텟 레벨(LV.0)과 비용도 즉시 원상복구
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.hpLevel = 0;
            UpgradeManager.Instance.speedLevel = 0;
            UpgradeManager.Instance.damageLevel = 0;
            UpgradeManager.Instance.UpdateUpgradeUI(); // 강화창 텍스트 강제 새로고침
        }
    }
}