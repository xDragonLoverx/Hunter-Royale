using System;
using System.Collections.Generic;
using UnityEngine;

public class FightAgentNode : MonoBehaviour
{
    //  --ACTIONS--

    Action<FightAgentNode> onSetCameFrom;
    Action<float> onSetGScore;
    Action<float> onSetHScore;

    //  --COMPONENTS--

    [SerializeField] List<FightAgentNode> connectedNodes = new List<FightAgentNode>();

    //  --VALUES--

    FightAgentNode cameFrom;
    float gScore;
    float hScore;

    //  --GETTERS--

    public Action<FightAgentNode> OnSetCameFrom { get { return onSetCameFrom; } }
    public Action<float> OnSetGScore {  get { return onSetGScore; } }
    public Action<float> OnSetHScore { get { return onSetHScore; } }
    public List<FightAgentNode> ConnectedNodes { get { return connectedNodes; } }
    public FightAgentNode CameFrom { get { return cameFrom; } }
    public float GScore { get { return gScore; } }
    public float HScore { get {return hScore; } }
    public float FScore { get { return gScore + hScore; } }

    //  ===UNITY METHODS===

    void OnEnable()
    {
        onSetCameFrom = (node) => cameFrom = node;
        onSetGScore = (value) => gScore = value;
        onSetHScore = (value) => hScore = value;
    }
    void OnDisable()
    {
        onSetCameFrom = null;
        onSetGScore = null;
        onSetHScore = null;
    }

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
    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.white;
    //    foreach (FightAgentNode node in connectedNodes)
    //    {
    //        Gizmos.DrawLine(transform.position, node.transform.position);
    //    }
    //    Gizmos.DrawWireSphere(transform.position, 0.5f);
    //}
}
