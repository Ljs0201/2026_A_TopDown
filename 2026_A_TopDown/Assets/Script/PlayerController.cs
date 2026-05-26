using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Sprite Sheets")]
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;

    [Header("Animation Settings")]
    public float frameTime = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerStatus playerStatus; // 실시간 스탯(이동속도 등)을 가져올 참조 변수

    private Vector2 input;
    private Vector2 velocity;

    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        // 같은 오브젝트에 있는 PlayerStatus 컴포넌트를 가져옵니다.
        playerStatus = GetComponent<PlayerStatus>();

        currentSprites = spriteDown;
        if (currentSprites != null && currentSprites.Length > 0)
        {
            sr.sprite = currentSprites[0];
        }
    }

    private void Update()
    {
        // 입력이 없을 때 (정지 상태) 애니메이션을 첫 프레임으로 고정
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            if (currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[frameIndex];
            }
            return;
        }

        // 애니메이션 타이머 진행
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;

            // 프레임 인덱스가 배열 범위를 벗어나면 처음으로 루프
            if (currentSprites != null && frameIndex >= currentSprites.Length)
            {
                frameIndex = 0;
            }

            if (currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[frameIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        // [중요 수정] 기존 고정 moveSpeed 대신, PlayerStatus의 성장형 실시간 moveSpeed를 반영합니다.
        if (playerStatus != null)
        {
            velocity = input.normalized * playerStatus.moveSpeed;
        }

        // 물리 기반 이동 처리
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();

        // 입력이 있을 때만 방향 전환 판단
        if (input.sqrMagnitude > 0.01f)
        {
            // X축 이동량이 Y축 이동량보다 클 경우 (좌우 이동)
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                    ChangeSprites(spriteRight);
                else
                    ChangeSprites(spriteLeft);
            }
            // Y축 이동량이 더 클 경우 (상하 이동)
            else
            {
                if (input.y > 0)
                    ChangeSprites(spriteUp);
                else
                    ChangeSprites(spriteDown);
            }
        }
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        // 스프라이트 배열이 비어있다면 무시 (에러 방지)
        if (newSprites == null || newSprites.Length == 0) return;

        // 이미 같은 방향의 스프라이트 시트를 재생 중이라면 무시
        if (currentSprites == newSprites)
            return;

        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        sr.sprite = currentSprites[frameIndex];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 대상이 몬스터 프리팹이라면 즉시 첫 피해 적용
        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();
            if (enemy != null && playerStatus != null)
            {
                // 실시간으로 안전하게 PlayerStatus의 TakeDamage 호출
                // (OnTriggerStay2D가 주 동력이 되므로 여기서는 확인차 넣어두거나 비워두어도 무방합니다)
            }
        }
    }
}