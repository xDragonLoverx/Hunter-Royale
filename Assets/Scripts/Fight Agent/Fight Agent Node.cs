using System.Collections.Generic;
using UnityEngine;

public class FightAgentNode : MonoBehaviour
{
    //  --COMPONENTS--

    [SerializeField] List<FightAgentNode> connectedNodes = new List<FightAgentNode>();

    //  --VALUES--

    float gScore;
    float hScore;

    //  --GETTERS--

    public List<FightAgentNode> ConnectedNodes { get { return connectedNodes; } }
    public float GScore { get { return gScore; } }
    public float HScore { get {return hScore; } }
    public float FScore { get { return gScore + hScore; } }

    //  ===UNITY METHODS===

    //void OnDrawGizmos()
    //{
    //    foreach (FightAgentNode node in connectedNodes)
    //    {
    //        if (node.ConnectedNodes.Contains(this))
    //        {
    //            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
    //        }
    //        else
    //        {
    //            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
    //        }
    //        Gizmos.DrawLine(transform.position, node.transform.position);
    //    }
    //    if(connectedNodes.Count > 0)
    //    {
    //        Gizmos.color = Color.aquamarine;
    //    }
    //    else
    //    {
    //        Gizmos.color = Color.red;
    //    }
    //    Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0f, 0.5f));
    //}
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        foreach (FightAgentNode node in connectedNodes)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
