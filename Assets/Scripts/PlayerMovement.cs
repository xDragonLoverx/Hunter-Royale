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
    private void Start()
    {
        RB = GetComponent<Rigidbody>();
        RB.freezeRotation = true;
    }
    private void Update()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + 0.2f, Ground);

        MyInput();
        SpeedControl();

        if (Grounded) { RB.linearDamping = GroundDrag; }
        else { RB.linearDamping = 0; }
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
    private void SpeedControl()
    {
        Vector3 FlatVelocity = new Vector3(RB.linearVelocity.x, 0f, RB.linearVelocity.z);

        if (FlatVelocity.magnitude > MoveSpeed)
        {
            Vector3 limitedVelocity = FlatVelocity.normalized * MoveSpeed;
            RB.linearVelocity = new Vector3(limitedVelocity.x, 0, limitedVelocity.z);
        }
    }

    //Freeze Agent Debuff
    private bool isFrozen = false;

    public void SetFrozen(bool frozenState)
    {
        isFrozen = frozenState;

        if (isFrozen && RB != null)
        {
            RB.linearVelocity = new Vector3(0f, RB.linearVelocity.y, 0f);
        }
    }
}
