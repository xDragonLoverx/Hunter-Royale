using UnityEngine;

public class KillMenu : MonoBehaviour
{
    GameObject Agent;
    private void Awake()
    {
        Agent = GameObject.Find("FawnAgent");
    }
    public void Option1()
    {
        Agent.GetComponent<FawnAgent>().KillChoice(0);
    }
    public void Option2()
    {
        Agent.GetComponent<FawnAgent>().KillChoice(1);
    }
    public void Option3()
    {
        Agent.GetComponent<FawnAgent>().KillChoice(2);
    }
}
