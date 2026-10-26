using System.Collections.Generic;
using UnityEngine;
public class WaypointNetworkManager : MonoBehaviour
{
    [Header("Corridor Settings")]
    [SerializeField] private float maxConnectionDistance = 12f;
    [SerializeField] private float agentClearanceRadius = 0.35f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float raycastHeightOffset = 0.5f;
    [ContextMenu("Auto Connect All Waypoints ")]
    public void ConnectWaypoints()
    {
        WaypointNode[] nodes = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
        Debug.Log($"[NetworkManager] Found {nodes.Length} nodes. Checking clearance connections...");

        int totalConnections = 0;

        foreach (var node in nodes)
        {
            if (node.neighbors == null) node.neighbors = new List<WaypointNode>();
            node.neighbors.Clear();
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            for (int j = i + 1; j < nodes.Length; j++)
            {
                WaypointNode a = nodes[i];
                WaypointNode b = nodes[j];

                float dist = Vector3.Distance(a.transform.position, b.transform.position);

                if (dist <= maxConnectionDistance && dist > 0.05f)
                {
                    Vector3 start = a.transform.position + Vector3.up * raycastHeightOffset;
                    Vector3 end = b.transform.position + Vector3.up * raycastHeightOffset;
                    Vector3 dir = (end - start).normalized;
                    Ray ray = new Ray(start, dir);
                    bool hitWall = Physics.SphereCast(ray, agentClearanceRadius, dist, obstacleLayer);

                    if (!hitWall)
                    {
                        a.neighbors.Add(b);
                        b.neighbors.Add(a);
                        totalConnections += 2;
                    }
                }
            }
        }

        Debug.Log($"[NetworkManager] Done! Linked {totalConnections / 2}");
    }
}