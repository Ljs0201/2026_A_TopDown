using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float originalSpeed = 1.0f; // 적의 원래 기본 이동속도
    private float currentSpeed;

    void Start()
    {
        currentSpeed = originalSpeed;
    }

    void Update()
    {
        // 원래 이속 대신 currentSpeed 변수를 사용하여 플레이어를 추적하게 만드세요!
        // transform.Translate(방향 * currentSpeed * Time.deltaTime);
    }

    // 아케인 존이 호출할 슬로우 버프 함수
    public void ApplySlow(float multiplier)
    {
        currentSpeed = originalSpeed * multiplier; // 기본 속도의 50% 등으로 다운
    }

    // 장판 밖으로 나가면 속도 원상복구
    public void RestoreSpeed()
    {
        currentSpeed = originalSpeed;
    }

    public void TakeDamage(float amount)
    {
        // 기존에 만들어두신 적 데미지 입는 로직을 여기에 연동하세요!
        Debug.Log($"{gameObject.name}이 장판에 맞아 {amount}의 피해를 입음!");
    }
}