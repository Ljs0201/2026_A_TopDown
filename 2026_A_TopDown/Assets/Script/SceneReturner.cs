using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReturner : MonoBehaviour
{
    public void ForceReturnToLobby()
    {
        Debug.Log("[씬 이동 치트키] 시간을 풀고 등록된 로비로 이동을 시도합니다.");

        // 1. 일시정지 상태 완전 해제
        Time.timeScale = 1f;

        // 2. ★ 중요: 유니티 프로젝트 창에 있는 실제 씬 파일 이름과 
        // 띄어쓰기, 대소문자(L, S)까지 100% 똑같아야 합니다!
        SceneManager.LoadScene("LobyScene");
    }
}