using UnityEngine;

public class BargainMenu : MonoBehaviour
{
    GameObject Agent;
    private void Awake()
    {
        Agent = GameObject.Find("FawnAgent");
    }
    public void Option1()
    {
        Agent.GetComponent<FawnAgent>().BargainChoice(0);
    }
    public void Option2()
    {
        Agent.GetComponent<FawnAgent>().BargainChoice(1);
    }
    public void Option3()
    {
        Agent.GetComponent<FawnAgent>().BargainChoice(2);
    }
}
