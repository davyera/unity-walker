using UnityEngine;

public class IdleFootManager : FootMovementManager
{
    private TransformHistory transformHistory;

    private float restDistance;

    [SerializeField]
    private float verticalRestAngle = 10f;
    [SerializeField]
    private float floatDriftSpeed = 10f;
    private float floatDriftAmount = 0;
    private bool drifting = false;

    private void Start()
    {
        Manager.player.state.OnUnGrounded += HandleUnGrounded;
        Manager.player.state.OnGrounded += HandleGrounded;

        transformHistory = GetComponent<TransformHistory>();
        UpdateAttributes();
    }

    private void Update()
    {
        ApplySwimTransitionEffects();
        UpdateAttributes();
        UpdateFloatDriftAmount();
        Debug.DrawRay(transform.position, transform.up * 5, Color.magenta);
    }

    private void ApplySwimTransitionEffects()
    {
        float lerp = Manager.player.GetSwimTransitionProgress();
        if (lerp > 0)
        {
            Quaternion parentRotation = Manager.player.transform.rotation;
            float angle = Mathf.Lerp(0, 90, lerp);
            Quaternion swimRotation = Quaternion.AngleAxis(angle, Manager.player.transform.right);
            transform.rotation = swimRotation * parentRotation;
        }
    }

    private void UpdateFloatDriftAmount()
    {
        bool inDescent = Manager.GetControllerVelocity().y < 0;
        bool isPlayerSwimming = Manager.player.state.IsSwimming();

        if (drifting && inDescent || isPlayerSwimming)
        {
            if (floatDriftAmount < 1)
                // When drifting, floatDriftAmount should approach 1
                floatDriftAmount = Mathf.Min(floatDriftAmount + floatDriftSpeed * Time.deltaTime, 1);
        }
        else
        {
            if (floatDriftAmount > 0)
                // When grounded, floatDriftAmount should approach 0 (twice as fast)
                floatDriftAmount = Mathf.Max(floatDriftAmount - 2 * floatDriftSpeed * Time.deltaTime, 0);
        }
    }

    private void HandleUnGrounded() 
    {
        drifting = true;
        transformHistory.StartTracking();
    }
    private void HandleGrounded() 
    { 
        drifting = false;
        transformHistory.StopTracking();
    }

    private void UpdateAttributes()
    {
        restDistance = Manager.GetBoundRadius() * (1.5f + floatDriftAmount);
    }

    public Vector3 GetRestPosition(float angleFromForward)
    {
        Vector3 restDirection = transform.forward;
        restDirection = Quaternion.AngleAxis(verticalRestAngle, -transform.right) * restDirection;
        restDirection = Quaternion.AngleAxis(angleFromForward, transform.up) * restDirection;
        return GetCenter() + restDirection * restDistance;
    }

    public Quaternion GetRestRotation(float angleFromForward)
    {
        Quaternion restRotation = GetPlayerRotation();

        // Rotate based on the drift amount -- when fully resting, tentacles will point inwards; when drifting, upwards
        float normalRotationAmount = Mathf.Lerp(180, 270, floatDriftAmount);

        restRotation = Quaternion.AngleAxis(normalRotationAmount, transform.right) * restRotation;
        restRotation = Quaternion.AngleAxis(angleFromForward, transform.up) * restRotation;

        // Rotate based on drag to approximate a ragdoll effect
        restRotation = GetDragRotation() * restRotation;

        return restRotation;
    }

    private Quaternion GetDragRotation()
    {
        Vector3 driftDirection = -transformHistory.GetPositionDelta().normalized;
        Quaternion dragRotation = Quaternion.FromToRotation(transform.up, driftDirection);
        return Quaternion.Lerp(Quaternion.identity, dragRotation, floatDriftAmount);
    }

    private Vector3 GetCenter()
    {
        Vector3 currentPosition = Manager.player.transform.position;
        Vector3 historyPositionDelta = transformHistory.GetPositionDelta();
        return currentPosition - historyPositionDelta * floatDriftAmount;
    }

    private Quaternion GetPlayerRotation()
    {
        Quaternion historyRotationDelta = Quaternion.Euler(transformHistory.GetRotationDelta());
        // we inverse the rotation as we will be "subtracting" it from the current rotation
        Quaternion historyEffect = Quaternion.Lerp(Quaternion.identity, Quaternion.Inverse(historyRotationDelta), floatDriftAmount);
        return transform.rotation * historyEffect;
    }
}
