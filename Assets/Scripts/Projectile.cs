using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float Speed;
    void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * Speed;
    }
    public void SetSpeed(float value)
    {
        Speed *= value;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9)
        {
            if (collision.gameObject.name == "FawnAgent")
            {
                collision.gameObject.GetComponent<FawnAgent>().Die();
            }
            else if (collision.gameObject.name == "FollowAgent")
            {
                collision.gameObject.GetComponent<FollowAgent>().Die();
            }
            else if (collision.gameObject.name == "FreezeAgent")
            {
                Destroy(collision.gameObject.transform.parent.gameObject);
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
        Destroy(gameObject);
    }
}
