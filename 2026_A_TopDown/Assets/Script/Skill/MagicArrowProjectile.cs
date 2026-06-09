using UnityEngine;

public class MagicArrowProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private float damage; // ★ 이 변수가 빠져있거나 위아래 이름이 달라서 에러가 났었습니다!
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
                // 변수 이름이 정확히 일치하므로 빨간 줄이 사라집니다.
                enemy.TakeDamage(damage);
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