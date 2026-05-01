using UnityEngine;

public class TestMapMarker : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        Gizmos.DrawLine(transform.position + Vector3.up * 2f, transform.position + Vector3.up * 2f + transform.forward);
    }
}
