using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 전환을 위해 반드시 필요합니다!

public class LobbyManager : MonoBehaviour
{
    /// <summary>
    /// 게임 시작 버튼을 누르면 호출할 함수입니다.
    /// </summary>
    public void ClickGameStart()
    {
        Debug.Log("게임 시작! PlayScene으로 이동합니다.");

        // "PlayScene"이라는 이름을 가진 씬으로 로드합니다.
        // (유저님의 실제 인게임 씬 이름과 완전히 일치해야 합니다!)
        SceneManager.LoadScene("PlayScene");
    }
}