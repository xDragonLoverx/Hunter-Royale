using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FightAgent : MonoBehaviour
{
    //  --ACTIONS--

    Action<Transform> onAlertAgent;
    Action onBecomeHunted;

    //  --CLASS ENUMS--

    enum FightAgentBehavior
    {
        Roaming,
        Investigating,
        Chasing,
        Fleeing,
        DestroyingObstacle
    }

    //  --COMPONENTS--

    [SerializeField] LayerMask playerLayerMask;
    [SerializeField] LayerMask obstacleLayerMask;

    Transform player;
    Vector3 targetDestination;
    List<Vector3> allNodes = new List<Vector3>();
    List<Vector3> pointsOfInterest = new List<Vector3>();

    //  --VALUES--

    const float MOVESPEED = 5f;
    const float ROTATESPEED = 10f;
    const float MINIDLETIME = 3f;
    const float MAXIDLETIME = 5f;
    const float MAXFLEEDISTANCE = 1000f;
    const float HEARINGCOOLDOWN = 5f;
    const float ATTACKCOOLDOWN = 1f;

    bool isInHuntingMode = true;
    bool nextDestinationDecided;
    bool canSeePlayer;
    bool canListenForNoise = true;
    bool canAttack = true;
    FightAgentBehavior currentBehavior;

    //  --GETTERS--

    public Action<Transform> OnAlertAgent { get { return onAlertAgent; } }
    public Action OnBecomeHunted { get { return onBecomeHunted; } }

    //  ===UNITY METHODS===

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        GameObject[] nodeCollection = GameObject.FindGameObjectsWithTag("Node");
        foreach(GameObject node in nodeCollection)
        {
            allNodes.Add(node.transform.position);
        }
    }
    void OnEnable()
    {
        onAlertAgent = AlertAgent;
        onBecomeHunted = () => isInHuntingMode = false;
    }
    void OnDisable()
    {
        onAlertAgent = null;
        onBecomeHunted = null;
    }
    void Update()
    {
        switch (currentBehavior)
        {
            case FightAgentBehavior.Roaming:
                StartCoroutine(Roam());
                break;
            case FightAgentBehavior.Investigating:
                StartCoroutine(Investigate());
                break;
            case FightAgentBehavior.Chasing:
                Chase();
                break;
            case FightAgentBehavior.Fleeing:
                Flee();
                break;
            case FightAgentBehavior.DestroyingObstacle:
                DestroyObstacle();
                break;
        }
        if(currentBehavior != FightAgentBehavior.Roaming && currentBehavior != FightAgentBehavior.Fleeing)
        {
            nextDestinationDecided = false;
        }
    }
    void FixedUpdate()
    {
        if(targetDestination != null)
        {
            StartCoroutine(LookAtTarget(targetDestination));
            transform.Translate(transform.forward * MOVESPEED * Time.deltaTime);
        }
    }

    //  ===CUSTOM METHODS===

    //  -Behavior-

    IEnumerator Roam()
    {
        if (!nextDestinationDecided)
        {
            targetDestination = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
            nextDestinationDecided = true;
            CreatePath();
        }
        else if (nextDestinationDecided && Vector3.Distance(transform.position, targetDestination) <= 0.1f)
        {
            nextDestinationDecided = false;
            yield return new WaitForSeconds(UnityEngine.Random.Range(MINIDLETIME, MAXIDLETIME));
        }
    }
    IEnumerator Investigate()
    {
        if(pointsOfInterest.Count > 0)
        {
            targetDestination = pointsOfInterest[0];
            if(Vector3.Distance(transform.position, targetDestination) <= 0.1f)
            {
                pointsOfInterest.RemoveAt(0);
                yield return new WaitForSeconds(UnityEngine.Random.Range(MINIDLETIME, MAXIDLETIME));
            }
            else
            {
                CreatePath();
            }
        }
        else
        {
            currentBehavior = FightAgentBehavior.Roaming;
        }
    }
    void Chase()
    {
        if (canSeePlayer)
        {
            targetDestination = player.position;
            if(canAttack && Physics.CheckSphere(transform.position, 1.5f, playerLayerMask))
            {
                StartCoroutine(Attack());
            }
            CreatePath();
        }
        else
        {
            pointsOfInterest.Insert(0, player.position);
            if(pointsOfInterest.Count > 5)
            {
                pointsOfInterest.RemoveAt(pointsOfInterest.Count - 1);
            }
            currentBehavior = FightAgentBehavior.Investigating;
        }
    }
    void Flee()
    {
        if (canSeePlayer && canAttack && Physics.CheckSphere(transform.position, 1.5f, playerLayerMask))
        {
            targetDestination = player.position;
            StartCoroutine(Attack());
        }
        else if(Vector3.Distance(transform.position, player.position) > MAXFLEEDISTANCE)
        {
            nextDestinationDecided = false;
            currentBehavior = FightAgentBehavior.Roaming;
        }
        else if(!nextDestinationDecided)
        {
            Vector3 furthestNode = allNodes[0];
            float furthestNodeDistance = 0f;
            foreach(Vector3 node in allNodes)
            {
                float distanceToMeasure = Vector3.Distance(transform.position, node);
                if(distanceToMeasure > furthestNodeDistance)
                {
                    furthestNode = node;
                    furthestNodeDistance = distanceToMeasure;
                }
            }
            targetDestination = furthestNode;
            nextDestinationDecided = true;
            CreatePath();
        }
    }
    void DestroyObstacle()
    {
        if (canAttack && Physics.CheckSphere(transform.position, 1.5f, obstacleLayerMask))
        {
            StartCoroutine(Attack());
        }
    }
    IEnumerator Attack()
    {
        Debug.Log("Attacka");

        canAttack = false;
        yield return new WaitForSeconds(ATTACKCOOLDOWN);
        canAttack = true;
    }

    //  -Navigation-

    void CreatePath()
    {

    }

    void CheckLineOfSight()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo))
        {
            if(hitInfo.transform == player)
            {
                canSeePlayer = true;
                if (isInHuntingMode)
                {
                    currentBehavior = FightAgentBehavior.Chasing;
                }
                else
                {
                    currentBehavior = FightAgentBehavior.Fleeing;
                }
            }
            else
            {
                canSeePlayer = false;
                if (hitInfo.transform.tag.Equals("Obstacle") && isInHuntingMode)
                {
                    currentBehavior = FightAgentBehavior.DestroyingObstacle;
                    targetDestination = hitInfo.transform.position;
                }
            }
        }
    }

    //  -Other-

    void AlertAgent(Transform objectOfInterest)
    {
        if (canListenForNoise && (currentBehavior == FightAgentBehavior.Roaming || currentBehavior == FightAgentBehavior.Investigating))
        {
            Debug.Log("Whoa something here");

            pointsOfInterest.Insert(0, objectOfInterest.position);
            if(pointsOfInterest.Count > 5)
            {
                pointsOfInterest.RemoveAt(pointsOfInterest.Count - 1);
            }
            currentBehavior = FightAgentBehavior.Investigating;
            StartCoroutine(HearingCooldown());
        }
    }
    IEnumerator LookAtTarget(Vector3 placeToLookAt)
    {
        Quaternion lookRotation = Quaternion.LookRotation(placeToLookAt - transform.position);
        float progress = 0f;
        while (progress < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, progress);
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);
            progress += Time.deltaTime * ROTATESPEED;
            yield return null;
        }
        CheckLineOfSight();
    }
    IEnumerator HearingCooldown()
    {
        canListenForNoise = false;
        yield return new WaitForSeconds(HEARINGCOOLDOWN);
        canListenForNoise = true;
    }
}