using UnityEngine;

/*
TODO:
ACCOUNT FOR WALLS WHEN MOVING (DONE SORT OF)
Finish searching for player (DONE)
Finish FindHidingSpot and Hunted scripts (DONE)
Integrate script with main to fix conflicts
*/
public enum Behavior{Hunted, Hunting, Wandering}

public class FlightAgent : MonoBehaviour
{
    [SerializeField] Behavior behavior;
    Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] float turnSpeed = 5f;
    [SerializeField] GameObject player;
    [SerializeField] Transform playerTransform;
    [SerializeField] float leapStrength;
    [SerializeField] float restCDTime;
    [SerializeField] float detectionRange;
    [SerializeField] float obstacleDistance = 1.5f;
    [SerializeField] float avoidAngle = 45f;
    [SerializeField] LayerMask wallLayer;
    public bool DoesPlayerHaveWeapon;
    float restingTime;
    bool isResting;
    float distance;
    Vector3 destination;
    bool foundHidingSpot;

    // Progress: Should be good
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Progress: Should be good
    void Update()   
    {
        if(!isResting) RunBehaviors();
        if (isResting)
        {
            restingTime += Time.deltaTime;
            if(restingTime >= restCDTime)
            {
                ToggleRest();
            }
        }
    }

    //Progress: Should be good, may need to figure out if Idle is needed
    void RunBehaviors()
    {
        switch (behavior){
            case Behavior.Hunted:
                Hunted();
                break;
            case Behavior.Hunting:
                Hunting();
                break;
            case Behavior.Wandering:
                Wandering();
                break;
        }
    }

    //Need to fix findHidingSpot but otherwise is good?
    void Hunted()
    {
        if (!foundHidingSpot)
        {
            destination = findHidingSpot(transform.position);
            MoveToDestination(destination);
        }
        MoveToDestination(destination);
        distance = Vector3.Distance(transform.position, destination);
        if(distance <= 0.5f)
        {
            ToggleRest();
            foundHidingSpot = false;
            behavior = Behavior.Wandering;
        }
    }

    //Progress: Done
    void Hunting()
    {
        distance = Vector3.Distance(transform.position, playerTransform.position);
        if(distance < 5f)
        {
            //perform leap attack
            Vector3 leapDirection = transform.forward * leapStrength;
            rb.AddForce(leapDirection, ForceMode.Impulse);
            //rest
            ToggleRest();
        }
        else
        {
            //keep chasing player
            destination = playerTransform.position;
            MoveToDestination(destination);
        }        
    }

    void Wandering()
    {
        SearchPlayer();
        if (!foundHidingSpot)
        {
            destination = findHidingSpot(transform.position);
            MoveToDestination(destination);
        }
        MoveToDestination(destination);
        distance = Vector3.Distance(transform.position, destination);
        if(distance <= 0.5f)
        {
            ToggleRest();
            foundHidingSpot = false;
            behavior = Behavior.Wandering;
        }
    }

    //Progress: Done
    void ToggleRest()
    {
        Debug.Log("resting");
        if (!isResting)
        {
            rb.linearVelocity = Vector3.zero;
            restingTime = 0;
            isResting = true;
        }
        else
        {
            isResting = false;
        }
    }

    //Progress: Works? Needs testing with moveToPosition
    Vector3 findHidingSpot(Vector3 position)
    {
        //initial best position is where agent currently is
        Vector3 bestPos = position;
        float furthestDistance = 0f;
        float searchSize = 10f;

        Vector3[] directions =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            new Vector3(1, 0, 1).normalized,
            new Vector3(-1, 0, 1).normalized,
            new Vector3(1, 0, -1).normalized,
            new Vector3(-1, 0, -1).normalized
        };

        for(int i = 1; i <= 10; i++)
        {
            //find new destination that goes further and further if possible
            foreach(Vector3 direction in directions)
            {
                Vector3 candidate = position + direction * (i * searchSize);
                RaycastHit hit;

                float pathDistance = 0f;
                //start at candidate, aim down, checks if it is hitting ground, then uses position for something idk
                if (Physics.Raycast(candidate, Vector3.down, out hit, 4f))
                {
                    Debug.Log("Tried to raycast: " + hit.point);
                    
                    if (!hit.collider.CompareTag("Ground"))
                    {
                        continue;
                    }
                    else
                    {
                        Debug.Log("Hit ground: " + hit.point);
                        candidate = hit.point;
                        pathDistance = Vector3.Distance(transform.position, candidate);
                    }
                }
                

                if (pathDistance > furthestDistance)
                {
                    furthestDistance = pathDistance;
                    bestPos = candidate;
                }
            }
        }
        Debug.Log(bestPos);
        foundHidingSpot = true;
        return bestPos;
    }

    //Progress: Working, but not perfect. Sometimes gets stuck on walls, but for a prototype it should be ok.
    void MoveToDestination(Vector3 destination)
    {
        Vector3 targetDir = destination - transform.position;
        targetDir.y = 0f;

        targetDir.Normalize();

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Vector3 leftDir = Quaternion.Euler(0f, -avoidAngle, 0f) * targetDir;

        Vector3 rightDir = Quaternion.Euler(0f, avoidAngle, 0f) * targetDir;

        bool blockedForward = Physics.Raycast(origin, targetDir, out RaycastHit forwardHit, obstacleDistance, wallLayer);

        bool blockedLeft = Physics.Raycast(origin, leftDir, out RaycastHit leftHit, obstacleDistance, wallLayer);

        bool blockedRight = Physics.Raycast(origin, rightDir, out RaycastHit rightHit, obstacleDistance, wallLayer);

        Vector3 moveDir = targetDir;

        if (blockedForward)
        {
            if (!blockedLeft && !blockedRight)
            {
                moveDir = leftHit.distance > rightHit.distance ? leftDir : rightDir;
            }
            else if (!blockedLeft)
            {
                moveDir = leftDir;
            }
            else if (!blockedRight)
            {
                moveDir = rightDir;
            }
            else
            {
                moveDir = Vector3.ProjectOnPlane(targetDir, forwardHit.normal).normalized;
            }
        }

        transform.position += moveDir * speed * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
    
    //Progress: not done yet? may want to account for specific behaviors
    void SearchPlayer()
    {
        distance = Vector3.Distance(transform.position, playerTransform.position);
        if(distance > detectionRange)
        {
            return;
        }
        RaycastHit hit;
        //Debug.DrawRay(transform.position, transform.forward * 15f, Color.red, 0.1f);
        if (Physics.Raycast(transform.position, transform.forward, out hit, 15f))
        {
            
            if (hit.collider.CompareTag("Player"))
            {
                if (!DoesPlayerHaveWeapon)
                {
                    Debug.Log("Flight Agent changing to Hunting");
                    ToggleBehavior(Behavior.Hunting);
                }
                else
                {
                    Debug.Log("Flight Agent changing to Hunted");
                    ToggleBehavior(Behavior.Hunted);
                }
            }
        }
    }
    
    //Progress: Done
    public void ToggleBehavior(Behavior newBehavior)
    {
        behavior = newBehavior;
    }
}
