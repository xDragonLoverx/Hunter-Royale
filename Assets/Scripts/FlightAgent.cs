using UnityEngine;
using UnityEngine.AI;

public enum Behavior{Hunted, Hunting, Idle}

public class FlightAgent : MonoBehaviour
{
    [SerializeField] Behavior behavior;
    NavMeshAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RunBehaviors()
    {
        switch (behavior){
            case Behavior.Hunted:
                Hunted();
                break;
            case Behavior.Hunting:
                Hunting();
                break;
            case Behavior.Idle:
                //Idle?
                break;
        }
    }

    void Hunted()
    {

    }

    void Hunting()
    {  

    }
}
