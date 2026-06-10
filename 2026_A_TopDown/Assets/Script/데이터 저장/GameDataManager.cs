using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("--- 데이터 아키텍처 컴포넌트 ---")]
    public GameSettingData gameSettingData;
    public SaveData saveData;

    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

            // ★ [핵심 추가] 게임이 시작될 때마다 기존에 있던 옛날 JSON 세이브 파일을 자동으로 삭제합니다.
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.LogWarning("[세이브 자동 초기화] 게임 시작 시 기존 JSON 파일을 삭제했습니다. 새 데이터로 시작합니다.");
            }

            // 파일을 지웠으므로, 무조건 1레벨 짜리 새 데이터를 깨끗하게 생성합니다.
            InitNewSaveData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveJsonData()
    {
        try
        {
            string jsonContext = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, jsonContext);
            Debug.Log($"[데이터 세이브 성공] 경로: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[데이터 세이브 실패]: {e.Message}");
        }
    }

    // [참고] 이제 Awake에서 시작하자마자 파일을 지우고 새로 만들기 때문에, 
    // LoadJsonData 함수는 게임 도중에 불러오기(스테이지 전환 등)용으로만 안전하게 작동합니다.
    public void LoadJsonData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonContext = File.ReadAllText(saveFilePath);
                saveData = JsonUtility.FromJson<SaveData>(jsonContext);
                Debug.Log("[데이터 로드 성공] 세이브 파일을 정상적으로 불러왔습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[데이터 로드 실패]: {e.Message}");
                InitNewSaveData();
            }
        }
        else
        {
            InitNewSaveData();
        }
    }

    // 최초 실행 및 초기화 시 10개 스킬의 이름표와 태초의 시작 레벨을 설정하는 함수
    private void InitNewSaveData()
    {
        saveData = new SaveData();
        saveData.skillSaveList = new List<SkillSaveInfo>();

        string[] skillNames = {
            "MagicArrow", "ArcaneZone", "FireBall", "Skill4", "Skill5",
            "Skill6", "Skill7", "Skill8", "Skill9", "Skill10"
        };

        for (int i = 0; i < 10; i++)
        {
            SkillSaveInfo info = new SkillSaveInfo();
            info.skillName = skillNames[i];

            // 매직 애로우(0번)만 기본 1레벨로 시작하고 나머지는 0레벨 세팅
            info.level = (i == 0) ? 1 : 0;

            saveData.skillSaveList.Add(info);
        }

        SaveJsonData();
    }

    private void OnApplicationQuit()
    {
        SaveJsonData();
    }
}