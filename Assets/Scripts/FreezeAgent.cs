using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAgent : MonoBehaviour
{
    public enum AgentMode
    {
        HUNTING,
        HUNTED
    }

    [Header("Agent Configuration")]
    [SerializeField] private AgentMode currentMode = AgentMode.HUNTING;
    [SerializeField] private float moveSpeed = 3.2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float arrivalRadius = 0.3f;
    [SerializeField] private float detectionRadius = 100f;

    [Header("Freeze Actuator (Debuff)")]
    [SerializeField] private float collisionRadius = 1.2f;
    [SerializeField] private float freezeDuration = 3.0f;
    [SerializeField] private float freezeCooldown = 10.0f;
    private float lastFreezeTime = -10.0f;

    [Header("Invisibility & Audio (Hunted Mode)")]
    [SerializeField] private AudioSource huntedAudioSource;
    private MeshRenderer[] renderers;
    private bool isCurrentlyVisible = true;

    [Header("Vision & Line of Sight")]
    [SerializeField] private float playerFieldOfView = 55f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float agentEyeHeight = 1.0f;

    [Header("Target & References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera playerCamera;

    private bool playerHasLineOfSight = false;
    private WaypointNode currentTargetNode;

    private readonly List<WaypointNode> allWaypoints = new List<WaypointNode>();
    private List<WaypointNode> currentPath = new List<WaypointNode>();
    private int pathIndex = 0;
    private float repathTimer = 0f;
    private const float REPATH_INTERVAL = 0.5f;

    private WaypointNode lastPlayerGoalNode;
    private AgentMode lastMode;

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        huntedAudioSource = GetComponent<AudioSource>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        FindPlayerReferences();
        CacheWaypoints();
        lastMode = currentMode;
    }

    void Start()
    {
        if (allWaypoints.Count == 0)
        {
            CacheWaypoints();
        }

        currentTargetNode = GetNearestWaypoint(transform.position);

        // Apply starting visibility based on mode
        UpdateVisibility(currentMode == AgentMode.HUNTING);

        if (playerTransform != null)
        {
            RecalculatePath();
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayerReferences();
            if (playerTransform == null) return;
        }

        // Mode switch detection
        if (currentMode != lastMode)
        {
            OnModeChanged(currentMode);
            lastMode = currentMode;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > detectionRadius)
        {
            return;
        }

        // HUNTING MODE 
        if (currentMode == AgentMode.HUNTING)
        {
            // Check freeze touch distance
            if (distanceToPlayer <= collisionRadius)
            {
                TryFreezePlayer();
            }

            playerHasLineOfSight = IsPlayerLookingAtMe();

            // Stop when player sees agent
            if (playerHasLineOfSight)
            {
                Debug.DrawLine(transform.position + Vector3.up * agentEyeHeight, playerCamera.transform.position, Color.red);
                return;
            }

            // Pursue player
            Debug.DrawLine(transform.position + Vector3.up * agentEyeHeight, playerTransform.position, Color.green);

            repathTimer += Time.deltaTime;
            if (repathTimer >= REPATH_INTERVAL || currentPath.Count == 0)
            {
                repathTimer = 0f;
                RecalculatePath();
            }

            FollowActivePath();
        }
        //  HUNTED MODE (Invisible & Fleeing) 
        else if (currentMode == AgentMode.HUNTED)
        {
            repathTimer += Time.deltaTime;
            if (repathTimer >= REPATH_INTERVAL || currentPath.Count == 0)
            {
                repathTimer = 0f;
                RecalculateFleePath();
            }

            FollowActivePath();
        }
    }

    // ACTUATOR 1: FREEZE / IMMOBILIZE PLAYER 
    private void TryFreezePlayer()
    {
        if (Time.time - lastFreezeTime >= freezeCooldown)
        {
            lastFreezeTime = Time.time;
            StartCoroutine(ApplyFreezeDebuff());
        }
    }

    private IEnumerator ApplyFreezeDebuff()
    {
        Debug.Log("<color=cyan>[FreezeAgent] IMMOBILIZING PLAYER!</color>");
        PlayerMovement[] moveScripts = playerTransform.GetComponentsInChildren<PlayerMovement>();
        foreach (var script in moveScripts)
        {
            script.enabled = false;
        }

        Rigidbody playerRb = playerTransform.GetComponentInChildren<Rigidbody>();
        if (playerRb == null) playerRb = playerTransform.GetComponentInParent<Rigidbody>();

        RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;
        if (playerRb != null)
        {
            originalConstraints = playerRb.constraints;
            playerRb.linearVelocity = Vector3.zero;
            // Freeze X and Z position so physical forces/slides cannot move the player
            playerRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        // Wait for freeze duration
        yield return new WaitForSeconds(freezeDuration);

        // --- RESTORE MOVEMENT ---
        foreach (var script in moveScripts)
        {
            if (script != null) script.enabled = true;
        }

        if (playerRb != null)
        {
            playerRb.constraints = originalConstraints;
        }

        Debug.Log("<color=yellow>[FreezeAgent] Player unfrozen.</color>");
    }

    // ACTUATOR 2: INVISIBILITY & MODE HANDLING 
    private void OnModeChanged(AgentMode newMode)
    {
        if (newMode == AgentMode.HUNTED)
        {
            UpdateVisibility(false); // Invisible

            if (huntedAudioSource != null && !huntedAudioSource.isPlaying)
            {
                huntedAudioSource.Play();
            }

            RecalculateFleePath();
        }
        else
        {
            UpdateVisibility(true); // Visible

            if (huntedAudioSource != null && huntedAudioSource.isPlaying)
            {
                huntedAudioSource.Stop();
            }

            RecalculatePath();
        }
    }

    private void UpdateVisibility(bool visible)
    {
        isCurrentlyVisible = visible;
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<MeshRenderer>();
        }

        foreach (var rend in renderers)
        {
            if (rend != null)
            {
                rend.enabled = visible;
            }
        }
    }

    // FLEE PATHFINDING (Hunted Mode) 
    private void RecalculateFleePath()
    {
        WaypointNode startNode = (currentTargetNode != null) ? currentTargetNode : GetNearestWaypoint(transform.position);
        WaypointNode furthestNode = GetFurthestWaypointFromPlayer();

        if (startNode == null || furthestNode == null) return;

        List<WaypointNode> fleePath = FindAStarPath(startNode, furthestNode);
        if (fleePath != null && fleePath.Count > 0)
        {
            currentPath = fleePath;
            pathIndex = 0;
            currentTargetNode = currentPath[0];
        }
    }

    private WaypointNode GetFurthestWaypointFromPlayer()
    {
        WaypointNode furthest = null;
        float maxDistance = -1f;

        foreach (var node in allWaypoints)
        {
            if (node == null) continue;
            float d = Vector3.Distance(playerTransform.position, node.transform.position);
            if (d > maxDistance)
            {
                maxDistance = d;
                furthest = node;
            }
        }
        return furthest;
    }

    // MOVEMENT & SENSORS 
    private void FollowActivePath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            return;
        }

        currentTargetNode = currentPath[pathIndex];
        if (currentTargetNode == null) return;

        Vector3 agentPos = transform.position;
        Vector3 nodePos = currentTargetNode.transform.position;

        Vector2 agent2D = new Vector2(agentPos.x, agentPos.z);
        Vector2 node2D = new Vector2(nodePos.x, nodePos.z);

        if (Vector2.Distance(agent2D, node2D) <= arrivalRadius)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count) return;

            currentTargetNode = currentPath[pathIndex];
            nodePos = currentTargetNode.transform.position;
        }

        Vector3 flatTarget = new Vector3(nodePos.x, agentPos.y, nodePos.z);
        transform.position = Vector3.MoveTowards(agentPos, flatTarget, moveSpeed * Time.deltaTime);

        Vector3 moveDir = (flatTarget - agentPos).normalized;
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void RecalculatePath()
    {
        WaypointNode goalNode = GetNearestWaypoint(playerTransform.position);
        if (goalNode == null) return;

        if (goalNode == lastPlayerGoalNode && currentPath != null && currentPath.Count > 0 && pathIndex < currentPath.Count)
        {
            return;
        }
        lastPlayerGoalNode = goalNode;

        WaypointNode startNode = (currentTargetNode != null) ? currentTargetNode : GetNearestWaypoint(transform.position);
        if (startNode == null) return;

        if (startNode == goalNode)
        {
            currentPath = new List<WaypointNode> { goalNode };
            pathIndex = 0;
            return;
        }

        List<WaypointNode> newPath = FindAStarPath(startNode, goalNode);
        if (newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            pathIndex = 0;
            currentTargetNode = currentPath[0];
        }
    }

    private List<WaypointNode> FindAStarPath(WaypointNode start, WaypointNode goal)
    {
        List<WaypointNode> openSet = new List<WaypointNode> { start };
        HashSet<WaypointNode> closedSet = new HashSet<WaypointNode>();
        Dictionary<WaypointNode, WaypointNode> cameFrom = new Dictionary<WaypointNode, WaypointNode>();

        Dictionary<WaypointNode, float> gScore = new Dictionary<WaypointNode, float> { [start] = 0f };
        Dictionary<WaypointNode, float> fScore = new Dictionary<WaypointNode, float> { [start] = Vector3.Distance(start.transform.position, goal.transform.position) };

        while (openSet.Count > 0)
        {
            WaypointNode current = openSet[0];
            float lowestF = fScore.ContainsKey(current) ? fScore[current] : Mathf.Infinity;

            for (int i = 1; i < openSet.Count; i++)
            {
                float f = fScore.ContainsKey(openSet[i]) ? fScore[openSet[i]] : Mathf.Infinity;
                if (f < lowestF)
                {
                    lowestF = f;
                    current = openSet[i];
                }
            }

            if (current == goal)
            {
                List<WaypointNode> path = new List<WaypointNode> { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Insert(0, current);
                }
                return path;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current.neighbors == null) continue;

            foreach (WaypointNode neighbor in current.neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor)) continue;

                float tentativeG = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (gScore.ContainsKey(neighbor) && tentativeG >= gScore[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                fScore[neighbor] = tentativeG + Vector3.Distance(neighbor.transform.position, goal.transform.position);
            }
        }

        return new List<WaypointNode> { start };
    }

    private bool IsPlayerLookingAtMe()
    {
        if (playerCamera == null) return false;

        Vector3 cameraPos = playerCamera.transform.position;
        Vector3 agentTargetPos = transform.position + Vector3.up * agentEyeHeight;
        Vector3 directionToAgent = (agentTargetPos - cameraPos).normalized;

        float angle = Vector3.Angle(playerCamera.transform.forward, directionToAgent);
        if (angle > playerFieldOfView)
        {
            return false;
        }

        float distance = Vector3.Distance(cameraPos, agentTargetPos);
        if (Physics.Raycast(cameraPos, directionToAgent, distance, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    private void FindPlayerReferences()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.Find("Player") ?? GameObject.Find("player");
            if (p != null)
            {
                playerTransform = p.transform;
            }
        }
        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("CameraObj");
            if (camObj != null)
            {
                playerCamera = camObj.GetComponentInChildren<Camera>();
            }
            else
            {
                playerCamera = Camera.main;
            }
        }
    }

    private void CacheWaypoints()
    {
        allWaypoints.Clear();
        allWaypoints.AddRange(FindObjectsByType<WaypointNode>(FindObjectsSortMode.None));
    }

    private WaypointNode GetNearestWaypoint(Vector3 position)
    {
        WaypointNode nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var node in allWaypoints)
        {
            if (node == null) continue;
            float d = Vector3.Distance(position, node.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                nearest = node;
            }
        }
        return nearest;
    }
}