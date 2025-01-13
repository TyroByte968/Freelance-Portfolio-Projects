using UnityEngine;

public class EnemyWaypoints : MonoBehaviour
{
    public Color WayPointColor;
    public float GizmoRadius = 5.0f;

    void Start()
    {

    }

    void OnDrawGizmos()
    {
        // Display the explosion radius when selected
        Gizmos.color = WayPointColor;
        Gizmos.DrawSphere(transform.position, GizmoRadius);
        
    }
}
