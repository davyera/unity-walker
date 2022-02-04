using UnityEngine;

public class WalkMovementComponent : ControllerMovementComponent
{
    [SerializeField]
    private bool strafingEnabled = true;
    [SerializeField]
    private float groundSpeed = 3f;
    [SerializeField]
    private float strafeVelocityEffect = 0.8f;
    [SerializeField]
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    [SerializeField]
    private float floatSmoothTime = 0.3f;

    private Vector3 smoothingVelocity;
    private float smoothVelocityTime;

    private void Start()
    {
        state.OnGrounded += HandleGrounded;
        state.OnUnGrounded += HandleUnGrounded;
    }

    private void HandleGrounded() { smoothVelocityTime = 0; }
    private void HandleUnGrounded() { smoothVelocityTime = floatSmoothTime; }

    public float GetVelocityMultiplier() { return groundSpeed; }

    private void Update()
    {
        float horizontal = input.ReadHorizontalInput();
        float vertical = input.ReadVerticalInput();

        // Apply strafing effects
        if (strafingEnabled)
        {
            // Effect the horizontal and backwards movement here if we are strafing
            horizontal *= strafeVelocityEffect;
            if (vertical < 0)
                vertical *= strafeVelocityEffect;
        }

        Vector3 localInput = new Vector3(horizontal, 0f, vertical);
        Vector3 cappedLocalInput = CapInput(localInput);
        Vector3 localDirection = cappedLocalInput.normalized;

        // Calculate controller angle (using directional input + camera direction)
        float localAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float targetAngle = localAngle + cam.eulerAngles.y;
        float smoothedTargetAngle = CalculateRotationAngle(targetAngle);

        Vector3 moveDirection = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;

        // Apply sliding limits to limit movement if we are moving "into" the slope
        Vector3 groundNormal = bounds.GetGroundNormal();
        if (state.IsSliding() && Vector3.Angle(moveDirection, groundNormal) > 90)
        {
            // Remove the y component from the ground normal and then normalize
            Vector3 groundNormalFlat = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            Vector3 moveProjection = Vector3.Project(moveDirection, groundNormalFlat);
            moveDirection -= moveProjection;
        }

        // Update state
        Vector3 newControllerVelocity = moveDirection * cappedLocalInput.magnitude * groundSpeed;
        Vector3 newVelocitySmoothed = SmoothVelocity(newControllerVelocity);
        controllerVelocity = newVelocitySmoothed;

        Quaternion newControllerRotation = Quaternion.Euler(0f, smoothedTargetAngle, 0f);
        controllerRotation = newControllerRotation;
    }

    private float CalculateRotationAngle(float controllerTargetAngle)
    {
        if (strafingEnabled)
            return cam.eulerAngles.y;
        return Mathf.SmoothDampAngle(transform.eulerAngles.y, controllerTargetAngle, ref turnSmoothVelocity, turnSmoothTime);
    }

    private Vector3 SmoothVelocity(Vector3 newControllerVelocity)
    {
        if (smoothVelocityTime > 0)
            return Vector3.SmoothDamp(controllerVelocity, newControllerVelocity, ref smoothingVelocity, smoothVelocityTime);
        else
            return newControllerVelocity;
    }
}
