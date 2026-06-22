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

    [Header("--- 영구 재화 전리품 세팅 ---")]
    [SerializeField] private int creditReward = 5; // ★ 이 몬스터를 잡으면 줄 크레딧 양 (인스펙터에서 수정 가능)

    private Transform playerTransform;
    private float attackTimer = 0f;

    // --- 스턴 관련 변수 ---
    private bool isStunned = false;
    private float stunTimer = 0f;

    // 애니메이터 대신 컴포넌트 없이 타격감을 줄 스프라이트 렌더러
    private SpriteRenderer spriteRenderer;

    private bool isDead = false; // 중복 사망 처리 방지용 변수

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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
        // ★ [스턴 체크] 스턴 상태라면 타이머를 깎고 아래 이동/공격 로직을 전부 패스
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                // 스턴이 풀리면 몬스터 색상을 원래대로(하얗게) 돌려놓습니다.
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
            }
            return; // 스턴 중일 때는 여기서 멈춤
        }

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

    // ★ 낙뢰 스킬에서 호출할 스턴 적용 함수
    public void ApplyStun(float duration)
    {
        isStunned = true;

        if (duration > stunTimer)
        {
            stunTimer = duration;
        }

        // 낙뢰를 맞으면 몬스터를 약간 푸르스름하게(전기 충격 느낌) 물들입니다.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.7f, 1f);
        }
    }

    // 매직 애로우나 낙뢰가 적을 적중시킬 때 호출하는 대미지 함수
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        hp -= amount;

        // 피가 0 이하가 되면 사망
        if (hp <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 스턴 상태일 때는 플레이어와 비벼져도 공격이 들어가지 않음
        if (isStunned) return;

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
        if (isDead) return;
        isDead = true;

        // 1. 기존 시스템 (경험치 및 킬수 추가)
        if (PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.GainExp(10f);
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.AddKill();
        }

        // ★ [핵심 추가] 크레딧 매니저를 호출하여 영구 보상 지급 및 기기 저장!
        if (PermanentCreditManager.Instance != null)
        {
            PermanentCreditManager.Instance.AddCredits(creditReward);
        }
        else
        {
            // 하이어라키에 매니저 배치를 깜빡했다면 경고를 띄웁니다.
            Debug.LogWarning("[PermanentCreditManager] 씬에 크레딧 매니저 오브젝트가 없습니다! 생성해 주세요.");
        }

        Destroy(gameObject);
    }
}