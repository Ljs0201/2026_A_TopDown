using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStatus playerStatus;
    private Animator animator;
    private SpriteRenderer sr;

    private Vector2 input;
    private Vector2 velocity;

    // 캐릭터가 마지막으로 바라본 가로 방향 (기본값: 오른쪽 = 1f)
    private float lastNonZeroX = 1f;

    [Header("Auto Attack Settings")]
    public float detectRange = 5f; // 자동으로 적을 탐지할 범위

    [Header("Magic Arrow Settings")]
    public GameObject magicArrowPrefab;
    public float arrowSpeed = 8f;
    public float arrowDamage = 15f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStatus = GetComponent<PlayerStatus>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        bool isMoving = input.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", input.x);
            animator.SetFloat("MoveY", input.y);

            lastNonZeroX = input.x;
            sr.flipX = (input.x < 0);
        }

        // 스페이스 바를 누르면 자동 조준 공격 시도
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        if (playerStatus != null)
        {
            velocity = input.normalized * playerStatus.moveSpeed;
        }
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
    }

    /// <summary>
    /// 자동으로 가장 가까운 적을 찾아 그 방향으로 애니메이션을 재생하고 화살을 발사하는 함수
    /// </summary>
    public void Attack()
    {
        // 1. 범위 내의 모든 몬스터(Collider)들을 수색합니다.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectRange);

        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        // 2. 검색된 오브젝트 중 "Enemy" 태그를 가진 가장 가까운 적을 찾습니다.
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector2.Distance(transform.position, enemyCollider.transform.position);
                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = enemyCollider.transform;
                }
            }
        }

        // ★ [핵심 수정] 범위 안에 '적(closestEnemy)이 존재할 때만' 모든 공격 연산을 실행합니다!
        if (closestEnemy != null)
        {
            // 플레이어에서 적을 향하는 방향 벡터 계산 후 정규화
            Vector3 lookDirection = closestEnemy.position - transform.position;
            Vector3 finalFireDirection = lookDirection.normalized;

            if (lookDirection.x != 0)
            {
                // 공격한 방향을 마지막 가로 방향으로 기억
                lastNonZeroX = lookDirection.x;

                // 애니메이터의 블렌드 트리 좌표를 적이 있는 방향(X, Y)으로 강제 갱신
                animator.SetFloat("MoveX", lookDirection.x);
                animator.SetFloat("MoveY", lookDirection.y);
            }

            // 적을 바라보게 되었으므로 이미지 뒤집기 적용
            sr.flipX = (lastNonZeroX < 0);

            // ★ 적이 있을 때만 애니메이터의 공격 트리거 발동!
            animator.SetTrigger("doAttack");

            // ★ 적이 있을 때만 매직 애로우 투사체 생성 및 발사!
            if (magicArrowPrefab != null)
            {
                GameObject arrowObj = Instantiate(magicArrowPrefab, transform.position, Quaternion.identity);
                MagicArrowProjectile arrowScript = arrowObj.GetComponent<MagicArrowProjectile>();

                if (arrowScript != null)
                {
                    bool isMaxLevel = false;
                    if (GameDataManager.Instance != null && GameDataManager.Instance.saveData.skillSaveList.Count > 0)
                    {
                        int currentLevel = GameDataManager.Instance.saveData.skillSaveList[0].level;
                        isMaxLevel = (currentLevel >= 5);
                    }

                    arrowScript.Setup(finalFireDirection, arrowSpeed, arrowDamage, isMaxLevel);
                }
            }
        }
        else
        {
            // 주변에 적이 없다면 로그를 찍고 아무 작업도 하지 않고 리턴합니다.
            Debug.Log("사거리 내에 적이 없어 공격을 발동하지 않습니다.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null && playerStatus != null)
            {
                // 피격 처리 등
            }
        }
    }
}