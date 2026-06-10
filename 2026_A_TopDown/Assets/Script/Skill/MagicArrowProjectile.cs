using UnityEngine;

public class MagicArrowProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private float damage;
    private bool canRicochet;
    private int ricochetCount = 0;

    public void Setup(Vector3 dir, float moveSpeed, float dmg, bool isMaxLevel)
    {
        moveDirection = dir.normalized;
        speed = moveSpeed;
        damage = dmg; // 받아온 대미지 저장
        canRicochet = isMaxLevel;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // ★ [핵심 추가] 매직 애로우(0번 인덱스)의 누적 데미지를 JSON 데이터에 실시간 기록
                // 데이터 매니저가 존재하고 스킬 리스트가 정상적으로 생성되어 있는지 확인 후 더해줍니다.
                if (GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 0)
                {
                    // 0번 슬롯(MagicArrow)의 accumulatedDamage에 현재 화살 데미지를 누적하고 저장!
                    GameDataManager.Instance.saveData.skillSaveList[0].accumulatedDamage += damage;
                    GameDataManager.Instance.SaveJsonData();
                }
            }

            if (canRicochet && ricochetCount < 1)
            {
                ricochetCount++;
                Transform nextTarget = FindRicochetTarget(collision.transform);

                if (nextTarget != null)
                {
                    moveDirection = (nextTarget.position - transform.position).normalized;
                    float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                    return;
                }
            }

            Destroy(gameObject);
        }
    }

    private Transform FindRicochetTarget(Transform currentEnemy)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        float minDistance = 2.0f;

        foreach (GameObject enemy in enemies)
        {
            if (enemy.transform == currentEnemy) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestTarget = enemy.transform;
            }
        }
        return bestTarget;
    }
}