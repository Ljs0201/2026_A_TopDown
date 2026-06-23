using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("--- Enemy Stats ---")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float baseDamage = 5f;    // ★ 보너스 가산을 위해 이름을 baseDamage로 변경
    private float finalDamage;                         // ★ 최종 적용 공격력
    [SerializeField] private float attackCooldown = 1f;

    public float maxHp = 30f;
    private float hp;

    [Header("--- 영구 재화 전리품 세팅 ---")]
    [SerializeField] private int creditReward = 5;

    private Transform playerTransform;
    private float attackTimer = 0f;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 만약 외부(스포너)에서 세팅을 안 해줬다면 기본 수치로 세팅
        if (finalDamage == 0) finalDamage = baseDamage;
        if (hp == 0) hp = maxHp;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// ★ 스포너가 소환과 동시에 원격 호출하여 몬스터 체급을 키우는 함수
    /// </summary>
    public void SetupBonusStats(float bonusHP, float bonusDamage)
    {
        this.maxHp += bonusHP;
        this.hp = this.maxHp; // 피를 보너스 수치만큼 채움

        this.finalDamage = this.baseDamage + bonusDamage;
    }

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
            }
            return;
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

    public void ApplyStun(float duration)
    {
        isStunned = true;
        if (duration > stunTimer)
        {
            stunTimer = duration;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.7f, 1f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        hp -= amount;
        if (hp <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isStunned) return;

        if (collision.CompareTag("Player"))
        {
            if (attackTimer <= 0f)
            {
                PlayerStatus playerStatus = collision.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    // ★ 기존 damage 변수 대신 난이도가 가산된 finalDamage로 가해집니다.
                    playerStatus.TakeDamage(finalDamage);
                    attackTimer = attackCooldown;
                }
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.GainExp(10f);
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.AddKill();
        }

        if (PermanentCreditManager.Instance != null)
        {
            PermanentCreditManager.Instance.AddCredits(creditReward);
        }
        else
        {
            Debug.LogWarning("[PermanentCreditManager] 씬에 크레딧 매니저 오브젝트가 없습니다! 생성해 주세요.");
        }

        Destroy(gameObject);
    }
}