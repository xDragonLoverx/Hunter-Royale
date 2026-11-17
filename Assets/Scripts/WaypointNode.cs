using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [SerializeField] public List<WaypointNode> neighbors = new List<WaypointNode>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);

        Gizmos.color = Color.green;
        if (neighbors != null)
        {
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
                }
            }
        }
    }
}