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

    float SpeedMultiplier = 1;
    float AttackMultiplier = 1;

    bool IsHunting = false;
    [SerializeField] float AttackTime;
    float AttackCount;

    [SerializeField] Transform ShotHome;
    [SerializeField] GameObject Projectile;
    GameObject ThisProjectile;
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
        if (IsHunting)
        {
            AttackCount += 1 * AttackMultiplier;
            if (AttackCount >= AttackTime && Input.GetKey(KeyCode.Space))
            {
                ThisProjectile = Instantiate(Projectile, ShotHome.position, ShotHome.rotation);
                ThisProjectile.GetComponent<Projectile>().SetSpeed(AttackMultiplier);
                Destroy(ThisProjectile, 3);
                AttackCount = 0;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Weapon")
        {
            collision.gameObject.SetActive(false);
            GameObject.Find("Main Camera").transform.GetChild(0).gameObject.SetActive(true);
            StartHunting();
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

        RB.AddForce(MoveDirection.normalized * MoveSpeed * SpeedMultiplier * 10f, ForceMode.Force);
    }
    private void SpeedControl(Vector3 flatVel)
    {

        if (flatVel.magnitude > MoveSpeed * SpeedMultiplier)
        {
            Vector3 limitedVelocity = flatVel.normalized * MoveSpeed * SpeedMultiplier;
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

    public void MakeNoise(float range)
    {
        Collider[] Nearby = Physics.OverlapSphere(transform.position, range, EnemyLayer);
        foreach (Collider c in Nearby)
        {
            if (c.gameObject.TryGetComponent(out FightAgent fightScript))
            {
                fightScript.OnAlertAgent?.Invoke(transform);
            }
            if (c.transform.parent.gameObject.name == "FawnAgent")
            {
                c.transform.parent.gameObject.GetComponent<FawnAgent>().Alert(transform);
            }
        }
    }
    void StartHunting()
    {
        IsHunting = true;
        GameObject.Find("FawnAgent").GetComponent<FawnAgent>().StartFleeing();
        GameObject.Find("Fight Agent").GetComponent<FightAgent>().OnBecomeHunted();
        GameObject.Find("FlightEnemy").GetComponent<FlightAgent>().ToggleBehavior(Behavior.Hunted);
        GameObject.Find("FollowAgent").GetComponent<FollowAgent>().StartFleeing();
        GameObject.Find("Freeze").GetComponent<FreezeAgent>().OnModeChanged(FreezeAgent.AgentMode.HUNTED);
    }
    public void SetSpeedModifier(float value) { SpeedMultiplier = value; }
    public void SetAttackModifier(float value) { AttackMultiplier = value; }
}
