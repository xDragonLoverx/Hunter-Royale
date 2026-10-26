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
    [SerializeField] LayerMask destructibleObstacleLayerMask;

    Transform player;
    FightAgentNode currentNode;
    List<FightAgentNode> currentPath = new List<FightAgentNode>();
    List<FightAgentNode> allNodes = new List<FightAgentNode>();
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
    bool canSeePlayer;
    bool canListenForNoise = true;
    bool canAttack = true;
    FightAgentBehavior currentBehavior;
    Transform placeToLookAt;

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
            if(node.TryGetComponent(out FightAgentNode nodeScript))
            {
                allNodes.Add(nodeScript);
            }
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
    void Start()
    {
        placeToLookAt = player;
    }
    void Update()
    {
        switch (currentBehavior)
        {
            case FightAgentBehavior.Roaming:
                //StartCoroutine(Roam());
                break;
            case FightAgentBehavior.Investigating:
                //StartCoroutine(Investigate());
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
        StartCoroutine(LookAtTarget(placeToLookAt));

        //if (currentPath.Count > 0)
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentPath[0].transform.position.x, 0f, currentPath[0].transform.position.z), 3 * Time.deltaTime);
        //    if (Vector3.Distance(transform.position, currentPath[0].transform.position) <= 0.1f)
        //    {
        //        currentPath.RemoveAt(0);
        //    }
        //}
        //else
        //{
        //    currentPath = CreatePath();
        //}
    }

    //  ===CUSTOM METHODS===

    //  -Behavior-

    //IEnumerator Roam()
    //{
    //    if (Vector3.Distance(transform.position, targetDestination) <= 0.1f)
    //    {
    //        yield return new WaitForSeconds(UnityEngine.Random.Range(MINIDLETIME, MAXIDLETIME));
    //        targetDestination = allNodes[UnityEngine.Random.Range(0, allNodes.Count)].transform.position;
    //    }
    //}
    //IEnumerator Investigate()
    //{
    //    if(pointsOfInterest.Count > 0)
    //    {
    //        if(Vector3.Distance(transform.position, pointsOfInterest[0]) <= 0.1f)
    //        {
    //            pointsOfInterest.RemoveAt(0);
    //            yield return new WaitForSeconds(UnityEngine.Random.Range(MINIDLETIME, MAXIDLETIME));
    //        }
    //    }
    //    else
    //    {
    //        currentBehavior = FightAgentBehavior.Roaming;
    //    }
    //}
    void Chase()
    {
        if (canSeePlayer)
        {
            placeToLookAt = player;
            if(canAttack && Physics.CheckSphere(transform.position, 1.5f, playerLayerMask))
            {
                StartCoroutine(Attack());
            }
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
            //targetDestination = player.position;
            StartCoroutine(Attack());
        }
        else if(Vector3.Distance(transform.position, player.position) > MAXFLEEDISTANCE)
        {
            currentBehavior = FightAgentBehavior.Roaming;
        }
    }
    void DestroyObstacle()
    {
        if (canAttack && Physics.CheckSphere(transform.position, 1.5f, destructibleObstacleLayerMask))
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

    //List<FightAgentNode> CreatePath()
    //{
    //    //  -Starting Node Setup-
    //    FightAgentNode startingNode = allNodes[0];
    //    float shortestDistance = float.MaxValue;
    //    foreach(FightAgentNode node in allNodes)
    //    {
    //        float distanceToMeasure = Vector3.Distance(transform.position, node.transform.position);
    //        if(distanceToMeasure < shortestDistance)
    //        {
    //            startingNode = node;
    //            shortestDistance = distanceToMeasure;
    //        }
    //    }
    //    //  -Ending Node Setup-
    //    FightAgentNode endingNode = allNodes[0];
    //    shortestDistance = float.MaxValue;
    //    foreach(FightAgentNode node in allNodes)
    //    {
    //        float distanceToMeasure = Vector3.Distance(targetDestination, node.transform.position);
    //        if(distanceToMeasure < shortestDistance && node != startingNode)
    //        {
    //            endingNode = node;
    //            shortestDistance = distanceToMeasure;
    //        }
    //    }
    //    //  -Pathfinding Setup-
    //    List<FightAgentNode> openSet = new List<FightAgentNode>();
    //    foreach(FightAgentNode node in allNodes)
    //    {
    //        node.OnSetGScore?.Invoke(float.MaxValue);
    //    }
    //    startingNode.OnSetGScore?.Invoke(0f);
    //    startingNode.OnSetHScore?.Invoke(Vector3.Distance(startingNode.transform.position, endingNode.transform.position));
    //    openSet.Add(startingNode);
    //    //  -Pathfinding Algorithm-
    //    while(openSet.Count > 0)
    //    {
    //        int lowestFScoreIndex = 0;
    //        foreach(FightAgentNode node in openSet)
    //        {
    //            if (openSet[openSet.IndexOf(node)].FScore < openSet[lowestFScoreIndex].FScore)
    //            {
    //                lowestFScoreIndex = openSet.IndexOf(node);
    //            }
    //        }
    //        FightAgentNode currentNode = openSet[lowestFScoreIndex];
    //        openSet.Remove(currentNode);
    //        if(currentNode == endingNode)
    //        {
    //            List<FightAgentNode> newPath = new List<FightAgentNode>();
    //            newPath.Insert(0, endingNode);
    //            while (currentNode != startingNode)
    //            {
    //                currentNode = currentNode.CameFrom;
    //                newPath.Add(currentNode);
    //            }
    //            newPath.Reverse();
    //            foreach(FightAgentNode node in newPath)
    //            {
    //                currentPath.Add(node);
    //            }
    //            return newPath;
    //        }
    //        foreach(FightAgentNode connectedNode in currentNode.ConnectedNodes)
    //        {
    //            float heldGScore = currentNode.GScore + Vector3.Distance(currentNode.transform.position, connectedNode.transform.position);
    //            if(heldGScore < currentNode.GScore)
    //            {
    //                connectedNode.OnSetCameFrom?.Invoke(currentNode);
    //                connectedNode.OnSetGScore?.Invoke(heldGScore);
    //                connectedNode.OnSetHScore?.Invoke(Vector3.Distance(connectedNode.transform.position, endingNode.transform.position));
    //                if (!openSet.Contains(connectedNode))
    //                {
    //                    openSet.Add(connectedNode);
    //                }
    //            }
    //        }
    //    }
    //    return null;
    //}

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
                if (hitInfo.transform.tag.Equals("Destructible Obstacle") && isInHuntingMode)
                {
                    currentBehavior = FightAgentBehavior.DestroyingObstacle;
                    placeToLookAt = hitInfo.transform;
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
    IEnumerator LookAtTarget(Transform placeToLookAt)
    {
        Quaternion lookRotation = Quaternion.LookRotation(placeToLookAt.position - transform.position);
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