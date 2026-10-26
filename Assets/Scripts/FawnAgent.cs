using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class FawnAgent : MonoBehaviour
{
    enum BehaviorMode
    {
        Searching, Chasing, Fleeing, Waiting  
    }
    BehaviorMode Mode;

    Transform[] Waypoints;
    Transform CurrentWaypoint;

    GameObject Player;
    int[] KillChoices;
    int[] BargainChoices;

    GameObject Canvas;
    [SerializeField] GameObject KillScreen;
    GameObject ThisKillScreen;
    [SerializeField] GameObject BargainScreen;
    GameObject ThisBargainScreen;

    [SerializeField] FawnNavigation Agent;
    [SerializeField] LayerMask SightLayer;
    private void Start()
    {
        Player = GameObject.Find("Player");
        Canvas = GameObject.Find("Canvas");
        Waypoints = new Transform[9];
        for (int i = 0; i < Waypoints.Length; i++)
        {
            Waypoints[i] = GameObject.Find($"FawnWaypoint{i}").transform;
        }

        CurrentWaypoint = transform;
        Mode = BehaviorMode.Searching;
        FindNewWaypoint();

        KillChoices = new int[] { 0, 1, -1 };
        BargainChoices = new int[] { 0, 1, -1 };
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) { StartFleeing(); }
        if (Input.GetKeyDown(KeyCode.B)) { Die(); }
        switch (Mode)
        {
            case BehaviorMode.Searching:
                if (Vector3.Distance(transform.position, CurrentWaypoint.position) < 3)
                {
                    FindNewWaypoint();
                }
                else if (Vector3.Angle(transform.forward, Vector3.Normalize(Player.transform.position - transform.position)) <= 45 && 
                    Physics.Raycast(transform.position, Vector3.Normalize(Player.transform.position - transform.position), out RaycastHit hit, 30, SightLayer))
                {
                    if (hit.collider.gameObject.layer == 6)
                    {
                        Mode = BehaviorMode.Chasing;
                    }
                }
                break;
            case BehaviorMode.Chasing:
                if (Vector3.Distance(transform.position, Player.transform.position) <= 6.5f)
                {
                    Attack();
                }
                else if (Vector3.Angle(transform.forward, Vector3.Normalize(Player.transform.position - transform.position)) <= 45 &&
                    Physics.Raycast(transform.position, Vector3.Normalize(Player.transform.position - transform.position), out RaycastHit hit, 30, SightLayer))
                {
                    if (hit.collider.gameObject.layer == 6)
                    {
                        Agent.SetDestination(Player.transform.position);
                    }
                    else
                    {
                        Mode = BehaviorMode.Searching;
                        CurrentWaypoint.position = Player.transform.position;
                    }
                }
                    break;
            case BehaviorMode.Fleeing:
                if (Vector3.Distance(transform.position, CurrentWaypoint.position) < 3)
                {
                    FindNewWaypoint();
                }
                break;
        }
    }
    void FindNewWaypoint()
    {
        Transform bestChoice = CurrentWaypoint;
        if (Mode == BehaviorMode.Fleeing)
        { bestChoice = ChooseWaypoint("Fleeing"); }
        else
        { bestChoice = ChooseWaypoint("Chasing"); }
        CurrentWaypoint = bestChoice;
        Agent.SetDestination(CurrentWaypoint.position);
    }
    Transform ChooseWaypoint(string destination)
    {
        float closest = 9999;
        float furthest = 0;
        Transform bestChoice = CurrentWaypoint;
        foreach (Transform t in Waypoints)
        {
            if ( t != CurrentWaypoint)
            {
                if (destination == "Chasing")
                {
                    if (Vector3.Distance(transform.position, t.position) + Vector3.Distance(Player.transform.position, t.position) < closest)
                    {
                        bestChoice = t;
                        closest = Vector3.Distance(transform.position, t.position) + Vector3.Distance(Player.transform.position, t.position);
                    }
                }
                else if (destination == "Fleeing")
                {
                    if (Vector3.Distance(Player.transform.position, t.position) - Vector3.Distance(transform.position, t.position) > furthest)
                    {
                        bestChoice = t;
                        furthest = Vector3.Distance(Player.transform.position, t.position) - Vector3.Distance(transform.position, t.position);
                    }
                }
                else if (destination == "Teleporting")
                {
                    if (Vector3.Distance(Player.transform.position, t.position) > furthest)
                    {
                        bestChoice = t;
                        furthest = Vector3.Distance(Player.transform.position, t.position);
                    }
                }
            }
        }
        return bestChoice;
    }
    void Attack()
    {
        if (Mode != BehaviorMode.Fleeing)
        {
            bool NoAttacksLeft = true;
            foreach (int i in KillChoices)
            {
                if (i != -1) { NoAttacksLeft = false; }
            }
            if (!NoAttacksLeft)
            {
                Mode = BehaviorMode.Waiting;
                ThisKillScreen = Instantiate(KillScreen, Canvas.transform);
                for (int i = 0; i < KillChoices.Length; i++)
                {
                    if (KillChoices[i] == -1)
                    {
                        ThisKillScreen.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    public void KillChoice(int choice)
    {
        KillChoices[choice] = -1;
        switch (choice)
        {
            case 0:
                Player.GetComponent<PlayerMovement>().SetSpeedModifier(0.75f);
                break;
            case 1:
                Player.transform.GetChild(3).gameObject.SetActive(true);
                GameObject.Find("Directional Light").GetComponent<Light>().intensity = 0;
                break;
            case 2:
                //GameObject.Find("FightAgent").GetComponent<FightAgent>().Alert(Player.transform.position);
                //GameObject.Find("FlightAgent").GetComponent<FlightAgent>().Alert(Player.transform.position);
                //GameObject.Find("FreezeAgent").GetComponent<FreezeAgent>().Alert(Player.transform.position);
                //GameObject.Find("FollowAgent").GetComponent<FollowAgent>().Alert(Player.transform.position);
                break;
        }
        Destroy(ThisKillScreen);
        Transform furthestPoint = ChooseWaypoint("Teleporting");
        Agent.enabled = false;
        GetComponent<Rigidbody>().position = new Vector3(furthestPoint.position.x, transform.position.y, furthestPoint.position.z);
        Agent.enabled = true;
        Mode = BehaviorMode.Searching;
        FindNewWaypoint();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void Die()
    {
        bool NoBargainsLeft = true;
        foreach (int i in BargainChoices)
        {
            if (i != -1) { NoBargainsLeft = false; }
        }
        if (!NoBargainsLeft)
        {
            Mode = BehaviorMode.Waiting;
            ThisBargainScreen = Instantiate(BargainScreen, Canvas.transform);
            for (int i = 0; i < BargainChoices.Length; i++)
            {
                if (BargainChoices[i] == -1)
                {
                    ThisBargainScreen.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void BargainChoice(int choice)
    {
        BargainChoices[choice] = -1;
        switch (choice)
        {
            case 0:
                Player.GetComponent<PlayerMovement>().SetSpeedModifier(1.25f);
                break;
            case 1:
                Player.GetComponent<PlayerMovement>().SetAttackModifier(1.5f); break;
            case 2:
                gameObject.transform.GetChild(1).gameObject.SetActive(true);
                //GameObject.Find("FightAgent").transform.GetChild(1).gameObject.SetActive(true);
                //GameObject.Find("FlightAgent").transform.GetChild(1).gameObject.SetActive(true);
                //GameObject.Find("FreezeAgent").transform.GetChild(1).gameObject.SetActive(true);
                //GameObject.Find("FollowAgent").transform.GetChild(1).gameObject.SetActive(true);
                break;
        }
        Destroy(ThisBargainScreen);
        Transform furthestPoint = ChooseWaypoint("Teleporting");
        Agent.enabled = false;
        GetComponent<Rigidbody>().position = new Vector3(furthestPoint.position.x, transform.position.y, furthestPoint.position.z);
        Agent.enabled = true;
        Mode = BehaviorMode.Fleeing;
        FindNewWaypoint();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void Alert(Transform location)
    {
        if (Mode != BehaviorMode.Fleeing)
        {
            if (Mode == BehaviorMode.Searching) { Mode = BehaviorMode.Chasing; }
            Agent.SetDestination(location.position);
            CurrentWaypoint = location;
        }
    }
    public void StartFleeing()
    {
        GameObject.Find("Directional Light").GetComponent<Light>().intensity = 2;
        Player.transform.GetChild(3).gameObject.SetActive(false);
        Player.GetComponent<PlayerMovement>().SetSpeedModifier(1);

        Mode = BehaviorMode.Fleeing;
        FindNewWaypoint();
    }
}
