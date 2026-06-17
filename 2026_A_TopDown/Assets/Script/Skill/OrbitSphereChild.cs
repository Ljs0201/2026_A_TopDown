using UnityEngine;

public class OrbitSphereChild : MonoBehaviour
{
    private ElementalSphere masterEffect;

    public void Setup(ElementalSphere master)
    {
        masterEffect = master;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (masterEffect == null) return;

        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // 1. 기본 데미지 연산
                float damage = masterEffect.GetCurrentDamage();
                enemy.TakeDamage(damage);

                float totalDmgThisHit = damage;

                // 2. 5레벨 달성 상태에서 1% 확률 광역 폭발 연산
                if (masterEffect.currentLevel >= 5)
                {
                    if (Random.value <= 0.01f)
                    {
                        totalDmgThisHit += TriggerSplashExplosion();
                    }
                }

                // ★ [데이터 관리] 이번 충돌로 발생한 총합 데미지를 skillSaveList[3]에 누적 후 저장
                if (GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 3)
                {
                    GameDataManager.Instance.saveData.skillSaveList[3].accumulatedDamage += totalDmgThisHit;
                    GameDataManager.Instance.SaveJsonData(); // 공용 JSON 파일 저장
                }
            }
        }
    }

    private float TriggerSplashExplosion()
    {
        float totalExplosionDmg = 0f;
        Vector3 explosionPosition = transform.position;

        if (masterEffect.explosionEffectPrefab != null)
        {
            GameObject exp = Instantiate(masterEffect.explosionEffectPrefab, explosionPosition, Quaternion.identity);
            Destroy(exp, 1.0f);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPosition, masterEffect.explosionRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyAI targetEnemy = collider.GetComponent<EnemyAI>();
                if (targetEnemy != null && collider.gameObject.activeSelf)
                {
                    targetEnemy.TakeDamage(masterEffect.explosionDamage);
                    totalExplosionDmg += masterEffect.explosionDamage;
                }
            }
        }

        return totalExplosionDmg;
    }
}