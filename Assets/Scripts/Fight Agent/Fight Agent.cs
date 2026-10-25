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

    const float MOVESPEED = 10f;
    const float ROTATESPEED = 10f;
    const float ATTACKCOOLDOWN = 1f;

    bool isInHuntingMode = true;
    [SerializeField] bool canSeePlayer;
    bool canAttack = true;
    [SerializeField] FightAgentBehavior currentBehavior;
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
    void Start()
    {
        targetDestination = player;
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
                if (isInHuntingMode && canSeePlayer)
                {
                    Chase();
                }
                break;
            case FightAgentBehavior.Fleeing:
                if (!isInHuntingMode && canSeePlayer)
                {
                    Flee();
                }
                break;
        }
        transform.LookAt(targetDestination);
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
            if(Physics.CheckSphere(transform.position, 1.5f, playerLayerMask))
            {
                if (canAttack)
                {
                    StartCoroutine(Attack());
                }
            }
        }
        else
        {

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



    //  -Other-

    void AlertAgent(Transform objectOfInterest)
    {
        if (currentBehavior == FightAgentBehavior.Roaming || currentBehavior == FightAgentBehavior.Investigating)
        {
            Debug.Log("Whoa something here");

            targetDestination = objectOfInterest;
        }
    }
}