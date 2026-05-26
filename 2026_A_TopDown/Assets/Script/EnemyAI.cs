using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("--- Enemy Stats ---")]
    [SerializeField] private float speed = 0.5f;       // 기획안 슬라임 속도
    [SerializeField] private float damage = 5f;        // 기획안 슬라임 공격력 (5)
    [SerializeField] private float attackCooldown = 1f; // 공격 주기 (1초에 한 번씩 공격)

    private Transform playerTransform;
    private float attackTimer = 0f;

    void Start()
    {
        // "Player" 태그를 가진 오브젝트를 스스로 추적
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // 공격 쿨타임 타이머 진행
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 플레이어 추적 이동
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

    // [핵심] 몬스터가 플레이어에게 대미지를 주는 물리 충돌 이벤트
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 부딪힌 대상이 "Player" 태그를 가지고 있다면
        if (collision.CompareTag("Player"))
        {
            // 공격 쿨타임이 끝났는지 확인
            if (attackTimer <= 0f)
            {
                // 플레이어의 PlayerStatus 스크립트를 가져옴
                PlayerStatus playerStatus = collision.GetComponent<PlayerStatus>();

                if (playerStatus != null)
                {
                    // 기획안의 대미지(5)만큼 체력을 깎음
                    playerStatus.TakeDamage(damage);

                    // 공격 타이머 리셋 (1초 동안 재공격 방지)
                    attackTimer = attackCooldown;
                }
            }
        }
    }
}