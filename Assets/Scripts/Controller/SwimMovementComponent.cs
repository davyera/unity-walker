using UnityEngine;

public class SwimMovementComponent : ControllerMovementComponent
{
    [SerializeField]
    private float swimSpeed = 9f;
    [SerializeField]
    private float rotateSmoothTime = 0.3f;
    [SerializeField]
    private float movementSmoothTime = 0.3f;
    private Vector3 movementSmoothingVelocity;

    private float rollAngle;
    private float rollAngleVelocity;

    private float pitchAngle;
    private float pitchAngleVelocity;
    
    void Update()
    {
        float horizontal = input.ReadHorizontalInput();
        float vertical = input.ReadVerticalInput();

        Vector3 localInput = new Vector3(horizontal, vertical, 0f);
        Vector3 cappedLocalInput = CapInput(localInput);
        Vector3 inputDirection = cappedLocalInput.normalized;

        // When swimming, we are always moving forward
        Vector3 localSwimDirection = new Vector3(inputDirection.x, inputDirection.y, 2f).normalized;
        Quaternion swimRotation = cam.rotation * Quaternion.Euler(localSwimDirection);

        Vector3 swimDirection = swimRotation * localSwimDirection;
        Vector3 newControllerVelocity = swimDirection * swimSpeed;
        Vector3 smoothedVelocity = Vector3.SmoothDamp(controllerVelocity, newControllerVelocity, ref movementSmoothingVelocity, movementSmoothTime);
        controllerVelocity = smoothedVelocity;

        // Add roll/pitch effects when turning
        Quaternion rollRotation =
            GetTwistRotation(swimDirection, transform.right, transform.forward, ref rollAngle, ref rollAngleVelocity);
        Quaternion pitchRotation =
            GetTwistRotation(swimDirection, transform.up, transform.right, ref pitchAngle, ref pitchAngleVelocity);

        Quaternion newControllerRotation = rollRotation * pitchRotation * swimRotation;
        controllerRotation = newControllerRotation;
    }

    private Quaternion GetTwistRotation(Vector3 movementDirection, 
                                        Vector3 effectDirection, 
                                        Vector3 rotationAxis, 
                                        ref float angle, 
                                        ref float angleVelocity)
    {
        Vector3 projectedMovement = Vector3.Project(movementDirection, effectDirection);
        float sign = -Vector3.Dot(projectedMovement, effectDirection);
        float targetAngle = sign * 180 * projectedMovement.magnitude;
        angle = Mathf.SmoothDampAngle(angle, targetAngle, ref angleVelocity, rotateSmoothTime);
        return Quaternion.AngleAxis(angle, rotationAxis);
    }
}
