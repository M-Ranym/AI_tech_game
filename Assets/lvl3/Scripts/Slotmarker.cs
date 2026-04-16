using UnityEngine;

public class SlotMarker : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, gameObject.name);
    }
#endif
}