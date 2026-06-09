using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("--- Enemy Stats ---")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float attackCooldown = 1f;

    // ★ 다른 UI나 프리팹 없이 오직 순수 체력 데이터만 추가!
    public float maxHp = 30f;
    private float hp;

    private Transform playerTransform;
    private float attackTimer = 0f;

    void Start()
    {
        // 게임 시작 시 현재 체력을 최대 체력으로 초기화
        hp = maxHp;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (direction.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // 매직 애로우가 적을 적중시킬 때 호출하는 대미지 함수
    public void TakeDamage(float amount)
    {
        hp -= amount;

        // 피가 0 이하가 되면 사망
        if (hp <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (attackTimer <= 0f)
            {
                PlayerStatus playerStatus = collision.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    playerStatus.TakeDamage(damage);
                    attackTimer = attackCooldown;
                }
            }
        }
    }

    void Die()
    {
        // 플레이어에게 슬라임 처치 보상 경험치(10점) 지급
        if (PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.GainExp(10f);
        }

        // UI 매니저의 킬 카운트 1 상승
        if (UIManager.instance != null)
        {
            UIManager.instance.AddKill();
        }

        // 몬스터 오브젝트 파괴
        Destroy(gameObject);
    }
}