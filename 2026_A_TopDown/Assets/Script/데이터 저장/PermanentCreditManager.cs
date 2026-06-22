using UnityEngine;

public class PermanentCreditManager : MonoBehaviour
{
    public static PermanentCreditManager Instance;

    [Header("--- 보유중인 영구 크레딧 ---")]
    public int permanentCredits = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 기기 내부에서 저장된 크레딧 로드
            permanentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);
            Debug.Log($"[크레딧 로드 완료] 현재 기기에 저장된 크레딧: {permanentCredits}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 외부에서 호출하면 크레딧을 무조건 더하고 저장합니다.
    /// </summary>
    public void AddCredits(int amount)
    {
        // 최신 정보 동기화 후 가산
        permanentCredits = PlayerPrefs.GetInt("PermanentCredits", 0);
        permanentCredits += amount;

        // 화면과 기기에 즉시 각인
        PlayerPrefs.SetInt("PermanentCredits", permanentCredits);
        PlayerPrefs.Save();

        // ★ 이 로그가 콘솔창에 찍히는지 눈으로 확인해야 합니다!
        Debug.Log($"<color=green>[크레딧 획득 성공]</color> +{amount}! 현재 보유량: {permanentCredits}");
    }
}