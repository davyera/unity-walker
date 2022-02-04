using UnityEngine;

public abstract class ControllerMovementComponent : MonoBehaviour, IMovementComponent
{
    protected ControllerInputListener input;
    protected PlayerStateController state;
    protected BoundsChecker bounds;
    protected Transform cam;

    private readonly float movementThreshold = 0.1f;

    protected Vector3 controllerVelocity;
    protected Quaternion controllerRotation = Quaternion.identity;

    private void Awake()
    {
        input = GetComponent<ControllerInputListener>();
        state = GetComponent<PlayerStateController>();
        bounds = GetComponent<BoundsChecker>();
        cam = Camera.main.transform;
    }

    /* We don't use direct positions for Controller Movement, just velocity */
    public Vector3 GetPosition() { return Vector3.zero; }
    public Vector3 GetVelocity() { return controllerVelocity; }
    public Quaternion GetRotation() { return controllerRotation; }
    public void StartUpdate() { if (!enabled) enabled = true; }
    public void StopUpdate() { if (enabled) enabled = false; }

    protected Vector3 CapInput(Vector3 input)
    {
        float magnitude = input.magnitude;
        if (magnitude > 1)
            return input.normalized;
        else if (magnitude < movementThreshold)
            return Vector3.zero;
        else
            return input;
    }
}
