using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    Vector3 PlayerPos;
    Vector3 WeaponPos;
    LinkedList<GameObject> Agents;
    bool PlayerIsHunting = false;

    [SerializeField] float heatRadius;
    [SerializeField] int HeatCap;
    int HeatValue = 0;
    [SerializeField] float HeatCooldown;
    bool CanHeatAuction = false;

    float Player_WeaponDist;
    [SerializeField] float HerdingDist;
    [SerializeField] float HerdingCooldown;
    bool CanHerdAuction = false;
    [SerializeField] GameObject[] Waypoints;
    [SerializeField] LayerMask SightLayer;

    void Update()
    {
        if (!PlayerIsHunting)
        {
            Player_WeaponDist = Vector3.Distance(PlayerPos, WeaponPos);
            if (Player_WeaponDist <= HerdingDist && CanHerdAuction)
            {
                CanHerdAuction = false;
                HerdingAuction();
                StartCoroutine(HerdAuctionCooldown());
            }
            if (HeatValue >= HeatCap && CanHeatAuction)
            {
                CanHeatAuction = false;
                HeatAuction();
                StartCoroutine(HeatAuctionCooldown());
            }
        }
    }
    private void FixedUpdate()
    {
        if (CanHeatAuction && !PlayerIsHunting)
        {
            CheckRadius(); //Checkradius placed in FixedUpdate so that the heat meter will not build build up at different rates depending on computer performance
        }
    }

    //Check how many agents are within a certain radius of the player, incrementing the heat meter for each;
    //if those agents also have line of sight, increment the meter more
    void CheckRadius()
    {
        foreach (GameObject agent in Agents)
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, PlayerPos);
            if (distanceToPlayer <= heatRadius)
            {
                HeatValue++;
            }
            if (Physics.Raycast(agent.transform.position, Vector3.Normalize(PlayerPos - agent.transform.position), out RaycastHit hit, heatRadius, SightLayer))
            {
                if (hit.collider.gameObject.layer == 10)
                {
                    HeatValue++;
                }
            }
        }
    }

    //Alert remaining agents when one has died, change their characteristics depending on how many are alive
    //Flight agent is coded differently, action needs to be used
    void AlertAgents()
    {
        
    }
    void FindFarRoom()
    {

    }
    
    //When the player gets too close to the weapon, find the closest agent to the weapon and bring them there
    void HerdingAuction()
    {

    }

    //Once the heat meter has maxed out, find the two closest agents and send them to patrol elsewhere
    void HeatAuction()
    {

    }

    //When player acquires the weapon and begins hunting the agent, update the enemy manager’s state
    public void SetPlayerHunting(bool isHunting) 
    {
        PlayerIsHunting = isHunting;
    }

    //Cooldown between when the heat auction can be called again
    private IEnumerator HeatAuctionCooldown()
    {
        yield return new WaitForSeconds(HeatCooldown);
        CanHeatAuction = true;
    }
    //Cooldown between when the heat auction can be called again
    private IEnumerator HerdAuctionCooldown()
    {
        yield return new WaitForSeconds(HerdingCooldown);
        CanHerdAuction = true;
    }

}
