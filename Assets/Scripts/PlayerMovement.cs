using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float MoveSpeed;
    [SerializeField] Transform Orientation;

    [SerializeField] float PlayerHeight;
    [SerializeField] LayerMask Ground;
    [SerializeField] float GroundDrag;
    bool Grounded;

    float HorizontalInput;
    float VerticalInput;

    Vector3 MoveDirection;
    Rigidbody RB;

    [SerializeField] LayerMask EnemyLayer;
    [SerializeField] float StepSoundRange;
    [SerializeField] int StepTime;
    int CurrentSteps;
    private void Start()
    {
        RB = GetComponent<Rigidbody>();
        RB.freezeRotation = true;
    }
    private void Update()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + 0.2f, Ground);

        Vector3 flatVelocity = new Vector3(RB.linearVelocity.x, 0f, RB.linearVelocity.z);

        MyInput();
        SpeedControl(flatVelocity);

        if (Grounded) { RB.linearDamping = GroundDrag; }
        else { RB.linearDamping = 0; }

        if (flatVelocity.magnitude > 0.5f)
        {
            CurrentSteps++;
        }
        if (CurrentSteps >= StepTime)
        {
            CurrentSteps = 0;
            MakeNoise(StepSoundRange);
        }
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void MyInput()
    {
        HorizontalInput = Input.GetAxisRaw("Horizontal");
        VerticalInput = Input.GetAxisRaw("Vertical");
    }
    private void MovePlayer()
    {
        MoveDirection = Orientation.forward * VerticalInput + Orientation.right * HorizontalInput;

        RB.AddForce(MoveDirection.normalized * MoveSpeed * 10f, ForceMode.Force);
    }
    private void SpeedControl(Vector3 flatVel)
    {

        if (flatVel.magnitude > MoveSpeed)
        {
            Vector3 limitedVelocity = flatVel.normalized * MoveSpeed;
            RB.linearVelocity = new Vector3(limitedVelocity.x, 0, limitedVelocity.z);
        }
    }
    public void MakeNoise(float range)
    {
        //Debug.Log("Step");
        Collider[] Nearby = Physics.OverlapSphere(transform.position, range, EnemyLayer);
        foreach (Collider c in Nearby)
        {
            /*
            if (c.gameObject.TryGetComponent<FightAgent>(out FightAgent script))
            {
                FightAgent.Alert(transform);
            }
            else if (c.gameObject.TryGetComponent<FlightAgent>(out FlightAgent script))
            {
                FlightAgent.Alert(transform);
            }
            else if (c.gameObject.TryGetComponent<FreezeAgent>(out FreezeAgent script))
            {
                FreezeAgent.Alert(transform);
            }
            else if (c.gameObject.TryGetComponent<FawnAgent>(out FawnAgent script))
            {
                FawnAgent.Alert(transform);
            }
            else if (c.gameObject.TryGetComponent<FollowAgent>(out FollowAgent script))
            {
                FollowAgent.Alert(transform);
            }
            */
        }
    }
    /* Sameple Alert Function
    public void Alert(Transform location)
    {
        [NavmeshAgent Object].SetDestination(location.position);
        [CurrentWaypoint Object] = location.gameObject;
    }
    */
}
