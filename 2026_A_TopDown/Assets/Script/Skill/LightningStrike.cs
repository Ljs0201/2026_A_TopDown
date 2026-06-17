using UnityEngine;
using System.Collections.Generic;

public class LightningStrike : SkillData
{
    [Header("--- 낙뢰 밸런스 세팅 ---")]
    public float baseDamage = 30f;
    public float damagePerLevel = 15f;
    public float strikeInterval = 2.0f;
    public float splashRadius = 2.0f;
    public float stunDuration = 0.3f;

    [Header("--- 이펙트 세팅 ---")]
    public GameObject lightningEffectPrefab;
    public float effectDestroyTime = 1.0f;

    // ★ [핵심 추가] 게임 시작 시 스킬의 잠금 상태를 제어할 플래그
    [Header("--- 해금 상태 ---")]
    public bool isUnlocked = false;

    private float timer = 0f;
    private Camera mainCamera;

    void Awake()
    {
        skillType = SkillType.LightningStrike;
        mainCamera = Camera.main;
    }

    void Start()
    {
        // 기본 1레벨 세팅을 유지하되, 게임 시작 시 타이머만 초기화합니다.
        timer = 0f;
    }

    void Update()
    {
        // ★ [핵심 변경] 레벨이 1이어도 'isUnlocked'가 false(잠금)라면 벼락이 치지 않습니다.
        if (!isUnlocked)
        {
            return;
        }

        // 해금된 상태(isUnlocked == true)일 때만 타이머가 흐르고 낙뢰 실행
        timer += Time.deltaTime;
        if (timer >= strikeInterval)
        {
            timer = 0f;
            TriggerLightningStrike();
        }
    }

    // ★ [외부 호출용 함수] 레벨업 UI에서 이 스킬을 '선택'했을 때 이 함수를 호출해 줍니다!
    public void UnlockOrLevelUp()
    {
        if (!isUnlocked)
        {
            // 처음 선택했다면 해금! (레벨은 기본 1레벨 유지)
            isUnlocked = true;
            currentLevel = 1;
            timer = 0f; // 해금 즉시 첫 발이 장전되도록 타이머 초기화
            Debug.Log("낙뢰 스킬이 최초 해금되었습니다! (Lv.1)");
        }
        else
        {
            // 이미 해금된 상태에서 또 선택했다면 레벨업!
            currentLevel++;
            Debug.Log($"낙뢰 스킬이 레벨업했습니다! (Lv.{currentLevel})");
        }
    }

    private void TriggerLightningStrike()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        List<GameObject> enemiesInScreen = new List<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeSelf) continue;

            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                if (GeometryUtility.TestPlanesAABB(cameraPlanes, enemyCollider.bounds))
                {
                    enemiesInScreen.Add(enemy);
                }
            }
        }

        if (enemiesInScreen.Count == 0) return;

        int strikeCount = currentLevel;
        float totalDamageThisFrame = 0f;

        for (int i = 0; i < strikeCount; i++)
        {
            if (enemiesInScreen.Count == 0) break;

            int randomIndex = Random.Range(0, enemiesInScreen.Count);
            GameObject targetEnemy = enemiesInScreen[randomIndex];
            enemiesInScreen.RemoveAt(randomIndex);

            if (targetEnemy != null && targetEnemy.activeSelf)
            {
                totalDamageThisFrame += ExecuteOneShotLightning(targetEnemy);
            }
        }

        if (totalDamageThisFrame > 0f && GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 2)
        {
            GameDataManager.Instance.saveData.skillSaveList[2].accumulatedDamage += totalDamageThisFrame;
            GameDataManager.Instance.SaveJsonData();
        }
    }

    private float ExecuteOneShotLightning(GameObject centerEnemy)
    {
        float currentDamage = baseDamage + (currentLevel - 1) * damagePerLevel;
        float totalDamageDealt = 0f;
        Vector3 strikePosition = centerEnemy.transform.position;

        if (lightningEffectPrefab != null)
        {
            GameObject effectInstance = Instantiate(lightningEffectPrefab, strikePosition, Quaternion.identity);
            Destroy(effectInstance, effectDestroyTime);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(strikePosition, splashRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyAI enemy = collider.GetComponent<EnemyAI>();
                if (enemy != null && collider.gameObject.activeSelf)
                {
                    enemy.TakeDamage(currentDamage);
                    totalDamageDealt += currentDamage;
                    enemy.ApplyStun(stunDuration);
                }
            }
        }

        return totalDamageDealt;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}