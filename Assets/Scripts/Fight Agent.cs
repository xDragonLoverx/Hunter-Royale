using System;
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
        Fleeing,
        Attacking
    }

    //  --COMPONENTS--
    
    [SerializeField] SphereCollider hearingRange;
    [SerializeField] SphereCollider attackingRange;

    Transform player;

    //  --VALUES--

    const float MOVESPEED = 10f;
    const float ROTATESPEED = 10f;

    bool isInHuntingMode;
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
                if (isInHuntingMode)
                {
                    Chase();
                }
                break;
            case FightAgentBehavior.Fleeing:
                if (!isInHuntingMode)
                {
                    Flee();
                }
                break;
            case FightAgentBehavior.Attacking:
                Attack();
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

    }
    void Flee()
    {

    }
    void Attack()
    {
        
        //else
        //{
        //    if (isInHuntingMode)
        //    {
        //        currentBehavior = FightAgentBehavior.Chasing;
        //    }
        //    else
        //    {
        //        currentBehavior = FightAgentBehavior.Fleeing;
        //    }
        //}
    }

    //  -Navigation-



    //  -Other-

    void AlertAgent(Transform objectOfInterest)
    {
        if (currentBehavior == FightAgentBehavior.Roaming || currentBehavior == FightAgentBehavior.Investigating)
        {
            targetDestination = objectOfInterest;
        }
    }
}