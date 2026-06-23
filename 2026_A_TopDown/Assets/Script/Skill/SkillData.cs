using UnityEngine;

public class SkillData : MonoBehaviour
{
    public enum SkillType { MagicArrow, ArcaneZone, LightningStrike, ElementalSphere }

    [Header("--- 스킬 기본 설정 ---")]
    public SkillType skillType = SkillType.MagicArrow;

    [Range(1, 5)]
    public int currentLevel = 1; // 인스펙터에서 1~5로 조절하며 테스트 가능!

    [Header("--- 밸런스 스탯 ---")]
    public float damage = 15f;         // 화살 한 발의 대미지
    public float attackInterval = 1.0f; // 발사 쿨타임 (1.2초)
    public float arrowSpeed = 5f;      // 화살 날아가는 속도

    [Header("--- ★ 스킬 고유 효과음 ★ ---")]
    public AudioClip skillSound; // ★ 이 스킬이 발사될 때 날 소리 파일 (.mp3 / .wav)
}