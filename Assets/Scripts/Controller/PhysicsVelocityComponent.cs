using UnityEngine;

public class PhysicsVelocityComponent : MonoBehaviour
{
    private PlayerStateController state;
    private PhysicsConstants physics;
    private WalkMovementComponent movement;
    private BoundsChecker bounds;

    private Vector3 jumpVelocity;
    private Vector3 gravityVelocity;
    private Vector3 slidingVelocity;

    [SerializeField]
    private float minSlideVelocity = 0.25f;

    private float jumpHeight;

    private void Start()
    {
        state = GetComponent<PlayerStateController>();
        movement = GetComponent<WalkMovementComponent>();
        bounds = GetComponent<BoundsChecker>();
        physics = PhysicsConstants.Instance;

        state.OnJumped += HandleJump;
        state.OnGrounded += HandleGround;
        state.OnHeadBlock += HandleHeadBlock;
        state.OnSlide += HandleSlide;
        state.OnSwimming += HandleSwimming;

        UpdateAttributes();
    }

    private void UpdateAttributes()
    {
        jumpHeight = GetComponent<CharacterController>().height * 2f;
    }

    public Vector3 Get()
    {
        Vector3 combinedVelocities = jumpVelocity + gravityVelocity + slidingVelocity;

        // Apply terminal velocity before returning
        combinedVelocities.y = Mathf.Max(combinedVelocities.y, physics.terminalVelocity);

        return combinedVelocities;
    }

    private void Update()
    {
        if (HasJumpVelocity())
        {
            Vector3 controllerVelocity = movement.GetVelocity();
            // Subtract lateral controller movement from jump velocity for next frame
            // If our controller velocity is moving against the jump velocity, dot product will be less than 0
            if (Vector3.Dot(controllerVelocity, jumpVelocity) < 0)
            {
                // Subtract the jump velocity projected onto movement to get the reduction
                jumpVelocity -= Vector3.Project(jumpVelocity, controllerVelocity);
            }
        }
    }

    private void FixedUpdate()
    {
        if (HasJumpVelocity())
        {
            // Apply drag to existing jump velocity
            Vector3 oldJumpDirection = jumpVelocity.normalized;
            float oldJumpMagnitude = jumpVelocity.magnitude;

            float dragMagnitude = Mathf.Max(oldJumpMagnitude - physics.drag * Time.deltaTime, 0);
            jumpVelocity = oldJumpDirection * dragMagnitude;
        }

        if (state.IsFloating() || state.IsSliding())
        {
            // Apply increasing gravity effects while we're floating or sliding
            gravityVelocity.y += physics.gravity * Time.deltaTime;
        }

        if (state.IsSliding())
        {
            Vector3 groundNormal = bounds.GetGroundNormal();
            Vector3 slideDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            float frictionMultiplier = (1f - physics.friction) * (1f - groundNormal.y);
            float slideSpeed = minSlideVelocity + frictionMultiplier;
            slidingVelocity = slideDirection * slideSpeed;
        }
        else
            slidingVelocity = Vector3.zero;
    }

    private void HandleJump()
    {
        gravityVelocity = Vector3.zero;

        // Derive initial jump direction as a mixture of current move direction. up, and ground normal
        Vector3 groundNormalDirection = bounds.GetGroundNormal();
        Vector3 groundMovementDirection = movement.GetVelocity().normalized;
        Vector3 jumpDirection = (Vector3.up + groundNormalDirection + groundMovementDirection).normalized;
        float jumpMagnitude = Mathf.Sqrt(jumpHeight * -2f * physics.gravity);
        jumpVelocity = jumpDirection * jumpMagnitude;
    }

    private void HandleGround()
    {
        jumpVelocity = Vector3.zero;
        gravityVelocity.y = physics.restingGravityVelocity;
    }

    private void HandleSwimming()
    {
        // TODO: gradually change this to zero, must be synced with rest of float -> swim transition
        jumpVelocity = Vector3.zero;
        gravityVelocity = Vector3.zero;
    }

    private void HandleSlide()
    {
        jumpVelocity = Vector3.zero;
    }

    private void HandleHeadBlock()
    {
        jumpVelocity.y = 0;
    }

    private bool HasJumpVelocity()
    {
        return jumpVelocity.magnitude != 0;
    }
}
