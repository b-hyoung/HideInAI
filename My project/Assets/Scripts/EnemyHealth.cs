using UnityEngine;

/// <summary>
/// 에너미 체력 관리. Capsule에 붙여서 사용.
/// 사용법: Capsule 오브젝트 만들고 이 스크립트 붙이기, Tag를 "Enemy"로 설정
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    private int currentHP;

    private Renderer bodyRenderer;
    private Color originalColor;

    private void Start()
    {
        currentHP = maxHP;
        bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer != null)
            originalColor = bodyRenderer.material.color;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"[에너미] {gameObject.name} 피격! 남은 HP: {currentHP}/{maxHP}");

        // 피격 시 빨간색으로 깜빡
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = Color.red;
            Invoke(nameof(ResetColor), 0.2f);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void ResetColor()
    {
        if (bodyRenderer != null)
            bodyRenderer.material.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"[에너미] {gameObject.name} 사망!");

        // 쓰러지는 연출 (옆으로 눕기)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 2초 후 제거
        Destroy(gameObject, 2f);
    }
}
