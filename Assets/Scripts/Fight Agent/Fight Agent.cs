using System;
using System.Collections;
using UnityEngine;
public class FightAgent : MonoBehaviour
{
    //  --ACTIONS--

    Action<Transform> onAlertAgent;

    //  --CLASS ENUMS--

    enum FightAgentBehavior
    {
        Roaming,
        Investigating,
        Chasing,
        Fleeing
    }

    //  --COMPONENTS--

    [SerializeField] LayerMask playerLayerMask;

    Transform player;

    //  --VALUES--

    const float MOVESPEED = 5f;
    const float ROTATESPEED = 10f;
    const float ATTACKCOOLDOWN = 1f;

    bool isInHuntingMode = true;
    bool canSeePlayer;
    bool canAttack = true;
    FightAgentBehavior currentBehavior;
    Transform targetDestination;

    //  --GETTERS--

    public Action<Transform> OnAlertAgent { get { return onAlertAgent; } }

    //  ===UNITY METHODS===

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
    }
    void OnEnable()
    {
        onAlertAgent = AlertAgent;
    }
    void OnDisable()
    {
        onAlertAgent = null;
    }
    void Update()
    {
        switch (currentBehavior)
        {
            case FightAgentBehavior.Roaming:
                Roam();
                break;
            case FightAgentBehavior.Investigating:
                Investigate();
                break;
            case FightAgentBehavior.Chasing:
                Chase();
                break;
            case FightAgentBehavior.Fleeing:
                Flee();
                break;
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

    //  -Behavior States-

    void Roam()
    {

    }
    void Investigate()
    {

    }
    void Chase()
    {
        if (canSeePlayer)
        {
            targetDestination = player;
            if(canAttack && Physics.CheckSphere(transform.position, 1.5f, playerLayerMask))
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            currentBehavior = FightAgentBehavior.Investigating;
        }
    }
    void Flee()
    {

    }
    IEnumerator Attack()
    {
        Debug.Log("Attacka");

        canAttack = false;
        yield return new WaitForSeconds(ATTACKCOOLDOWN);
        canAttack = true;
    }

    //  -Navigation-

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
            }
            if (hitInfo.transform.tag.Equals("Obstacle"))
            {
                targetDestination = hitInfo.transform;
            }
        }
    }

    //  -Other-

    void AlertAgent(Transform objectOfInterest)
    {
        if (currentBehavior == FightAgentBehavior.Roaming || currentBehavior == FightAgentBehavior.Investigating)
        {
            Debug.Log("Whoa something here");

            targetDestination = objectOfInterest;
        }
    }
    IEnumerator LookAtTarget(Transform objectToLookAt)
    {
        Quaternion lookRotation = Quaternion.LookRotation(objectToLookAt.position - transform.position);
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
}