using UnityEngine;

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

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 2f;
    public float stoppingDistance = 5f;
    [SerializeField] private float wallCheckDistance = 1.5f;
    private float turnAmount = 90f;

    
    [Header("Vision")]
    [SerializeField] private float visionAngle = 45f;
    
    private bool playerSeen = false;
    
    [Header("Waypoints")]
    Transform[] spawnWaypoints;
    Transform currWaypoint;


    void Start()
    {
        
    }


    void Update()
    {

        // Making sure player target is there
        if (!playerSeen || player == null)
        {
            return;
        }

        // Calculate enemy to player
        Vector3 dir = player.position - transform.position;

        // Keeping Y coord frozen and movement flat
        dir.y = 0f;

        float distanceToPlayer = dir.magnitude;

        // Handle rotating towards player direction
        if (dir != Vector3.zero)
        {
            Quaternion targetRotate = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotate, rotateSpeed * Time.deltaTime);
        }

        // Check if too close to player to keep follow behavior
        if (distanceToPlayer > stoppingDistance)
        {
            // Checks if maze walls are directly in front for movement
            if (WallBlocking())
            {
                // The turning
                transform.Rotate(0f, turnAmount * Time.deltaTime, 0f);


            }
            else
            {
                // Continue moving towards the player pos
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;

            }

        }

     }

    private bool WallBlocking()
    {
        Vector3 rayStart = transform.position + Vector3.up;

        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
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

        // Handle if player is outside of the designated vision cone
        if (angleToPlayer > visionAngle)
        {
            return false;
        }

        float distanceToPlayer = dirToPlayer.magnitude;

        // Beginning raycasting for obstacles starting above ground a little
        Vector3 rayStart = transform.position + Vector3.up;


        if (Physics.Raycast(rayStart, dirToPlayer.normalized, out RaycastHit hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return false;
            }
        }

            return true;

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
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            playerSeen = false;

            Debug.Log("Player left FOV");
        }
    }
    
    /*public bool Alert()
    {
        // sth sth other agents
    }

    public void Attack()
    {

    }
    */
}
