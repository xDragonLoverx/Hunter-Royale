using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class FawnNavigation : MonoBehaviour
{
    GameObject[] NavPoints;
    Vector3 Destination;
    Vector3 NextPoint;
    LinkedList<GameObject> PreviousPoints;
    [SerializeField] LayerMask SightLayer;

    Rigidbody RB;

    bool Moving;

    [SerializeField] float MoveSpeed;

    float StuckTime = 20;
    float StuckCounter = 0;
    void Awake()
    {
        PreviousPoints = new LinkedList<GameObject>();
        RB = GetComponent<Rigidbody>();
        NavPoints = new GameObject[GameObject.Find("Maze NavPoints").transform.childCount];
        for (int i = 0; i < NavPoints.Length; i++)
        {
            NavPoints[i] = GameObject.Find("Maze NavPoints").transform.GetChild(i).gameObject;
        }
        Moving = false;
    }
    private void Update()
    {
        float distanceToNext = Vector3.Distance(transform.position, NextPoint);
        if (Vector3.Distance(transform.position, Destination) <= 3)
        {
            Moving = false;
        }
        else if (Moving && distanceToNext > 0.5f)
        {
            transform.LookAt(NextPoint);
            RB.AddForce(transform.forward * MoveSpeed * 10f, ForceMode.Force);
        }
        else if (NextPoint != Destination)
        {
            ReachNextPoint();
        }
        if (Moving && RB.linearVelocity.magnitude < 1)
        {
            StuckCounter++;
            ReachNextPoint();
        }
        if (StuckCounter > StuckTime && PreviousPoints.Count > 2)
        {
            PreviousPoints.RemoveFirst();
            NextPoint = PreviousPoints.First.Value.transform.position;
            StuckCounter = 0;
        }
        SpeedControl();
    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(RB.linearVelocity.x, 0f, RB.linearVelocity.z);
        if (flatVel.magnitude > MoveSpeed)
        {
            Vector3 limitedVelocity = flatVel.normalized * MoveSpeed;
            RB.linearVelocity = new Vector3(limitedVelocity.x, 0, limitedVelocity.z);
        }
    }
    Vector3 FindNearbyNodes()
    {
        List<GameObject> Nearby = new List<GameObject>();
        GameObject Next = gameObject;

        int range = 10;

        foreach (GameObject point in NavPoints)
        {
            if (Vector3.Distance(transform.position, point.transform.position) <= range)
            {
                bool recentlyVisited = false;
                bool lineOfSight = true;
                foreach (GameObject g in PreviousPoints)
                {
                    if (g == point)
                    {
                        recentlyVisited = true;
                    }
                }
                if (Physics.Raycast(transform.position, Vector3.Normalize(point.transform.position - transform.position), range, SightLayer))
                {
                    lineOfSight = false;
                }
                if (point.transform.position == Destination)
                {
                    return point.transform.position;
                }
                else if (!recentlyVisited && lineOfSight && Vector3.Distance(transform.position, point.transform.position) > 3)
                {
                    Nearby.Add(point);
                }
            }
        }
        
        float lowest = 9999;
        foreach (GameObject point in Nearby)
        {
            float distance = Mathf.Sqrt(Vector3.Distance(transform.position, point.transform.position) * Vector3.Distance(transform.position, point.transform.position) +
                Vector3.Distance(point.transform.position, Destination) * 2.5f * Vector3.Distance(point.transform.position, Destination));
            if (distance < lowest)
            {
                Next = point;
                lowest = distance;
            }
        }
        if (PreviousPoints.Count > 20)
        {
            PreviousPoints.RemoveFirst();
        }
        if (Next != gameObject)
        {
            PreviousPoints.AddLast(Next);
            return Next.transform.position;
        }
        else
        {
            return Destination;
        }
    }
    void ReachNextPoint()
    {
        Moving = false;
        if (Vector3.Distance(transform.position, Destination) > 1)
        {
            NextPoint = FindNearbyNodes();
            Moving = true;
        }
    }
    public void SetDestination(Vector3 position)
    {
        PreviousPoints.Clear();
        Destination = position;
        NextPoint = FindNearbyNodes();
    }
    public void Warp(Vector3 position)
    {
        NextPoint = FindNearbyNodes();
    }
}
