using System;
using UnityEngine;

public class GizmoPosition : MonoBehaviour
{
    [SerializeField] private Color color;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
