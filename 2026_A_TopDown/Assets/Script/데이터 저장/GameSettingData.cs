using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingData", menuName = "Game Data/Game Setting Data")]
public class GameSettingData : ScriptableObject
{
    [Header("--- 플레이어 기본 스탯 ---")]
    public int startHp = 100;
    public int startAttack = 10;
    public float playerMoveSpeed = 1.2f;

    [Header("--- 사망 보너스 스탯 ---")]
    public int hpBonusPerDeath = 5;
    public int atkBonusPerDeath = 1;

    // [PPT 연동 핵심] 스킬들의 5단계 최고 만렙 제한 기획 수치
    [Header("--- 스킬 밸런스 세팅 ---")]
    public int maxSkillLevel = 5;
}