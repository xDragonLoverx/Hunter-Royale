using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FollowAgent : MonoBehaviour
{

    public enum mode
    {
        Hunting,
        Hunted
    }

    [Header("Mode")]
    public mode currMode = mode.Hunting;


    [Header("Target")]
    public Transform player;
    public Transform nearbyAgent;
    

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 2f;
    public float stoppingDistance = 5f;
    [SerializeField] private float wallCheckDistance = 1.5f;

    [Header("Attacking")]
    [SerializeField] private float stillTimeReq = 2f;
    [SerializeField] private float attackDistance = 6f;
    private Vector3 lastPlayerPos;
    private float stillTimer = 0f;


    
    [Header("Vision")]
    [SerializeField] private float visionAngle = 45f;
    
    [SerializeField] private bool playerSeen = false;



    [Header("Spawning")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private int amtToSpawn = 1;
    [SerializeField] private Transform[] spawnWaypoints;
    
    private static bool followExist = false;
    public int maxHP = 3;
    public int currHP;


    Transform currWaypoint;


    void Start()
    {
        currHP = maxHP;
        if (!followExist)
        {
            followExist = true;
            SpawnFollowAgent();
            
        }
    }


    void Update()
    {

        // Making sure player target is there
        if (player == null)
        {
            return;
        }


        if (currMode == mode.Hunting && !playerSeen)
        {
            return;
        }

        // Finds closest agent instead of seeking player

        if (currMode == mode.Hunted)
        {
            FindNearbyAgent();
        }


        // Calculate enemy to player
        Vector3 dir = player.position - transform.position; // Calculaton for distance only

        // Keeping Y coord frozen and movement flat
        dir.y = 0f;

        float distanceToPlayer = dir.magnitude;
        if (currMode == mode.Hunting)
        {
            CheckPlayerStationary();
            // Check if too close to player to keep follow behavior and deals with rotation
            if (distanceToPlayer > stoppingDistance)
            {
                // Pick best movement direction with hill climbing :)
                Vector3 bestDir = ChooseBestDirection(); // Enemy uses this to actually move

                if (bestDir != Vector3.zero)
                {
                    Quaternion moveRotation = Quaternion.LookRotation(bestDir);

                    transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, rotateSpeed * Time.deltaTime);

                    transform.position += bestDir * moveSpeed * Time.deltaTime;
                }

            }
        }
        else if (currMode == mode.Hunted)
        {

            Vector3 bestDir = ChooseBestDirection();

            if (bestDir != Vector3.zero)
            {
                Quaternion moveRotation = Quaternion.LookRotation(bestDir);

                transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, rotateSpeed * Time.deltaTime);

                transform.position += bestDir * moveSpeed * Time.deltaTime;
            }
        }
     }


    private void SpawnFollowAgent()
    {
        if (agentPrefab == null || spawnWaypoints.Length == 0)
        {
            return;
        }

        for (int i= 0; i< amtToSpawn; i++)
        {
            // Selects random point from array
            int randomInt = Random.Range(0, spawnWaypoints.Length);

            Transform spawnWaypoint = spawnWaypoints[randomInt];

            Instantiate(agentPrefab, spawnWaypoint.position, spawnWaypoint.rotation);
        }
    }

    private bool WallBlocking(Vector3 dir)
    {
        Vector3 rayStart = transform.position + Vector3.up;

        if (Physics.Raycast(rayStart, dir.normalized, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Ground"))
            return true;
        }

        return false;
    
    }

    // Makes cone of vision only front facing instead of using 360 degrees
    private bool IsPlayerInVision(Transform target)
    {

        Vector3 dirToPlayer = target.position - transform.position;

        // Keeps the y frozen - not relevant
        dirToPlayer.y = 0f;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        float distanceToPlayer = dirToPlayer.magnitude;


        // Beginning raycasting for obstacles starting above "Ground" a little
        Vector3 rayStart = transform.position + Vector3.up;


        /* For debugging - visible ray line
        Debug.DrawRay(rayStart, dirToPlayer.normalized * distanceToPlayer, Color.red);
        */
        
        // Handle if player is outside of the designated vision cone
        if (angleToPlayer > visionAngle)
        {
            return false;
        }

        if (Physics.Raycast(rayStart, dirToPlayer.normalized, out RaycastHit hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                return false;
            }
        }

            return true;

        }

    private Vector3 ChooseBestDirection()
    {
        Vector3[] directions =
        {
            transform.forward, Quaternion.Euler(0, 45, 0) * transform.forward, Quaternion.Euler(0, -45, 0) * transform.forward,
            Quaternion.Euler(0, 90, 0) * transform.forward, Quaternion.Euler(0, -90, 0) * transform.forward
        };

        Vector3 bestDir = Vector3.zero;

        float bestScore = Mathf.NegativeInfinity;

        foreach (Vector3 direction in directions)
        {
            // Checking if wall is in front and if so skipping the direction it is currently on
            if (WallBlocking(direction))
            {
                continue;
            }

            Vector3 possiblePos = transform.position + direction.normalized;

            float distanceToPlayer = Vector3.Distance(possiblePos, player.position);

            float score = 0f;

            // Scoring based on mode

            if (currMode == mode.Hunting)
            {
                score = -distanceToPlayer;
            }
            else if (currMode == mode.Hunted)
            {
                // Being further from player now gives reverse points
                score = distanceToPlayer;

                if (nearbyAgent != null)
                {
                    float distanceToAgent = Vector3.Distance(possiblePos, nearbyAgent.position);

                    // Adding value to being close to another agrnt
                    score -= distanceToAgent;

                }
            }

            // Randomizing a tad for stochastic reasons
            score += Random.Range(0f, 0.5f);
            
            // Makes best decision
            if (score > bestScore)
            {
                bestScore = score;
                bestDir = direction;
            }
        }

        return bestDir.normalized;
    }

    // Checking with sphere collider
    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            Transform target = other.attachedRigidbody.transform;
            if (IsPlayerInVision(target))
            {
                if (!playerSeen)
                {
                    // For testing
                    Debug.Log("Player found.");
                }
                playerSeen = true;
                player = target;
            }
            else
            {
                playerSeen = false;
                stillTimer = 0f;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            playerSeen = false;
            stillTimer = 0f;

            Debug.Log("Player left FOV");
        }
    }

    private void FindNearbyAgent() 
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");

        float closestDistance = Mathf.Infinity;

        Transform closestAgent = null;

        foreach (GameObject agent in agents)
        {
            // Ignores self as a target
            if (agent.transform == transform)
            {
                continue;
            }
            
            float distance = Vector3.Distance(transform.position, agent.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;

                closestAgent = agent.transform;
            }
        }
        nearbyAgent = closestAgent;

    }

    private void CheckPlayerStationary()
    {
        if (player == null)
        {
            return;
        }

        float moveAmt = Vector3.Distance(player.position, lastPlayerPos);

        // In case player barely moved
        if (moveAmt < 0.01f)
        {
            stillTimer += Time.deltaTime;
        }
        else
        {
            stillTimer = 0f;
        }

        lastPlayerPos = player.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Checking player diatance and time stood still requirement to attack
        if (stillTimer >= stillTimeReq && distanceToPlayer <= attackDistance)
        {
            Attack();

            stillTimer = 0f;
        }
    }

    public void Attack()
    {
        Debug.Log("Follow Agent attacks player.");
    }

    public void Attacked(int dmg)
    {
        currHP -= dmg;

        Debug.Log("Follow Agent was attacked. Current HP: " + currHP);
        
        if (currHP > 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Debug.Log("Folow Agent has died.");

        Destroy(this.gameObject);
    }
    public void StartFleeing()
    {
        currMode = mode.Hunted;
    }
    
    /*public bool Alert()
    {
        //
    }
    */
}
