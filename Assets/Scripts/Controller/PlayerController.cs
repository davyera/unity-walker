using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;

    [SerializeField]
    private float swimTimeoutDuration = 0.5f;
    private TimeoutCounter swimTimeout;

    [SerializeField]
    private FootManager footManager;
    [SerializeField]
    private TriggerCollider groundAnticipation;

    public PlayerStateController state;
    private ControllerInputListener input;
    private BoundsChecker bounds;
    private JumpMovementEffector jumpMovementEffector;
    private SlopeMovementEffector slopeMovementEffector;
    private PhysicsVelocityComponent physicsVelocity;
    private SwimMovementComponent swimMovement;
    private WalkMovementComponent walkMovement;

    [SerializeField]
    private float swimTransitionTime = 1f;
    private MovementTransition swimTransition;
    private MovementBlend movement;

    void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked

        state = GetComponent<PlayerStateController>();
        input = GetComponent<ControllerInputListener>();
        bounds = GetComponent<BoundsChecker>();
        jumpMovementEffector = GetComponent<JumpMovementEffector>();
        slopeMovementEffector = GetComponent<SlopeMovementEffector>();
        physicsVelocity = GetComponent<PhysicsVelocityComponent>();
        swimMovement = GetComponent<SwimMovementComponent>();
        walkMovement = GetComponent<WalkMovementComponent>();
        InitSwimTransition();

        swimTimeout = new TimeoutCounter(swimTimeoutDuration);
    }

    private void Start()
    {
        groundAnticipation.Init();
        groundAnticipation.OnEnter += HandleGroundAnticipation;
    }

    private void InitSwimTransition()
    {
        swimTransition = gameObject.AddComponent<MovementTransition>();
        swimTransition.Init(swimTransitionTime, false);
        movement = swimTransition.RegisterMovementTransition(walkMovement, swimMovement);
    }

    private void HandleGroundAnticipation(GameObject obj) 
    {
        if (state.IsSwimming() && !swimTimeout.IsTimedOut())
            TriggerSwimStop();
    }

    // Calculates where the "ground" position should be depending on controller height and scale
    public Vector3 GetControllerGroundPosition()
    {
        Vector3 heightDifference = new Vector3(0, GetDistanceToGround() + controller.skinWidth, 0);
        return transform.position - heightDifference;
    }

    public Vector3 GetGroundedPosition() { return bounds.GetGroundPosition(); }
    public float GetDistanceToGround() { return controller.height / 2 * transform.localScale.y; }
    public float GetMaxSpeed() { return walkMovement.GetVelocityMultiplier(); }
    public float GetSwimTransitionProgress() { return swimTransition.GetProgress(); }
    public MovementTransition GetSwimTransition() { return swimTransition; }

    private void Update()
    {
        CheckJumpSwimInput();

        // Apply rotation to character
        transform.rotation = movement.GetRotation();

        // Derive and apply final movement to character
        // Add controller specific component
        Vector3 controllerVelocity = movement.GetVelocity();
        Vector3 finalControllerVelocity = controllerVelocity * slopeMovementEffector.Get() * jumpMovementEffector.Get();

        // Add physics component
        Vector3 finalPhysicsVelocity = physicsVelocity.Get();

        // Add foot-derived component
        Vector3 derivedFootVelocity = footManager.GetFootVelocityComponent();

        Vector3 finalVelocity = finalControllerVelocity + finalPhysicsVelocity + derivedFootVelocity;
        Vector3 finalMovement = finalVelocity * Time.deltaTime;

        controller.Move(finalMovement);
    }

    private void CheckJumpSwimInput()
    {
        bool jumpKeyPressed = input.ReadJumpRequest();

        if (state.IsGrounded())
        {
            if (jumpKeyPressed)
                TriggerJump();
        }

        else if (state.IsFloating())
        {
            if (input.IsSwimKeyHeld() && !swimTimeout.IsTimedOut())
                TriggerSwim();
        }

        else if (state.IsSwimming())
        {
            if (!input.IsSwimKeyHeld())
                TriggerSwimStop();
        }
    }

    private void TriggerJump()
    {
        swimTimeout.StartCounter();
        state.TriggerJumped();
    }

    private void TriggerSwim()
    {
        swimTimeout.StartCounter();
        swimTransition.Increment();

        // TODO: transition to Swim movement
        state.SetGroundState(PlayerGroundState.SWIMMING);
        state.TriggerSwimming();
    }

    private void TriggerSwimStop()
    {
        swimTimeout.StartCounter();
        swimTransition.Decrement();

        // TODO: transition to Float movement
        state.SetGroundState(PlayerGroundState.FLOATING);
        state.TriggerSwimStop();
    }

    private void OnDrawGizmos()
    {
        if (state == null) return;

        Color debugColor;
        if (state.IsGrounded()) debugColor = Color.green;
        else if (state.IsFloating()) debugColor = Color.blue;
        else if (state.IsSwimming()) debugColor = Color.cyan;
        else debugColor = Color.red;
        Gizmos.color = debugColor;

        Gizmos.DrawSphere(transform.position + transform.up, 0.1f);
        if (bounds != null)
            DrawDebugLine(bounds.GetGroundNormal(), 3, debugColor);
    }

    private void DrawDebugLine(Vector3 direction, float multiplier, Color color)
    {
        Debug.DrawLine(transform.position, transform.position + direction * multiplier, color);
    }
}
