using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SkillSaveInfo
{
    public string skillName;
    public int level;
    public float accumulatedDamage; // ★ [핵심 추가] 이 스킬이 가한 총 누적 데미지!
}

[System.Serializable]
public class SaveData
{
    public int deathCount = 0;
    public List<SkillSaveInfo> skillSaveList = new List<SkillSaveInfo>();
}