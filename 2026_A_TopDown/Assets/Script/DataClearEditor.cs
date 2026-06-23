#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DataClearEditor
{
    [MenuItem("치트키/모든 데이터 완벽 초기화")]
    public static void ClearAllGameData()
    {
        PlayerPrefs.DeleteAll(); // 크레딧, 스텟 레벨, 수치 전부 초기화
        PlayerPrefs.Save();
        Debug.LogWarning("<color=red><b>[초기화 완료]</b></color> 모든 데이터가 리셋되었습니다!");
    }
}
#endif