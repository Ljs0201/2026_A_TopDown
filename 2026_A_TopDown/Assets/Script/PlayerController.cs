using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Sprite Sheets")]
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;

    [Header("Animation Settings")]
    public float frameTime = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;

    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    private void Update()
    {
        // 입력이 없을 때 (정지 상태) 애니메이션을 첫 프레임으로 고정
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
            return;
        }

        // 애니메이션 타이머 진행
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;

            // 프레임 인덱스가 배열 범위를 벗어나면 처음으로 루프
            if (frameIndex >= currentSprites.Length)
            {
                frameIndex = 0;
            }

            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        // 물리 기반 이동 처리
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

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
        // 이미 같은 방향의 스프레시트를 재생 중이라면 무시
        if (currentSprites == newSprites)
            return;

        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        sr.sprite = currentSprites[frameIndex];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*어떤 기능
        // <Summary>
        // 
        // 

        if (collision.CompareTag("Item"))
        {
            ItemObject item = collision.GetComponent<ItemObject>();

            score += item.GetPoint();

            GameDataManager.instance.playerData.collectedItems.Add(item.GetItem());

            scoreText.text = score.ToString();
            Destroy(collision.gameObject);

            GameDataManager.instance.SaveData(GameDataManager.instance.playerData);
        }
        */
    }
}