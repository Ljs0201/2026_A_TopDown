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

    public void ClickReturnToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobyScenes");
    }
}