using Photon.Pun;
using UnityEngine;

/// <summary>
/// 에너미 체력 관리. Capsule에 붙여서 사용.
/// 멀티플레이: 모든 데미지 요청은 MasterClient로 위임, MasterClient가 적용 후 결과를 모든 클라이언트에 동기화.
/// </summary>
public class EnemyHealth : MonoBehaviourPun
{
    [SerializeField] private int maxHP = 3;
    private int currentHP;

    private Renderer bodyRenderer;
    private Color originalColor;
    private bool dead;

    private void Start()
    {
        currentHP = maxHP;
        bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer != null)
            originalColor = bodyRenderer.material.color;
    }

    /// <summary>
    /// 외부에서 호출하는 데미지 진입점. PhotonView 있으면 MasterClient에 위임.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (photonView != null && PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, damage);
        }
        else
        {
            ApplyDamageAuthoritative(damage);
        }
    }

    [PunRPC]
    private void RPC_RequestDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamageAuthoritative(damage);
    }

    private void ApplyDamageAuthoritative(int damage)
    {
        if (dead) return;
        int newHP = Mathf.Max(0, currentHP - damage);

        if (photonView != null && PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, newHP);
        }
        else
        {
            ApplyHPAndEffects(newHP);
        }
    }

    [PunRPC]
    private void RPC_SyncHP(int newHP)
    {
        ApplyHPAndEffects(newHP);
    }

    private void ApplyHPAndEffects(int newHP)
    {
        currentHP = newHP;
        Debug.Log($"[에너미] {gameObject.name} 피격! 남은 HP: {currentHP}/{maxHP}");

        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = Color.red;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.2f);
        }

        if (currentHP <= 0 && !dead)
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
        dead = true;
        Debug.Log($"[에너미] {gameObject.name} 사망!");
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Destroy(gameObject, 2f);
    }
}
