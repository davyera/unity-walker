using UnityEngine;

public class FloatStep : StepBehaviour
{
    // "Memory" data helps smooth movement when transitioning between approaching land and drifting
    private Vector3 targetPosition;
    private Vector3 targetPositionMemory;
    private Vector3 targetNormal;
    private Vector3 targetNormalMemory;

    // Drift movement smoothing
    private float centerBias = 1;
    private float centerBiasSmoothVelocity;
    private float maxLandDistance;

    public FloatStep(FootWalkManager manager, Transform initialState, FootRange footRange)
        : base(manager, initialState, footRange) {}

    protected override void DoUpdate()
    {
        UpdateTargets();

        float movementLerp = manager.JumpCurve.Evaluate(lerp);

        position = Vector3.Lerp(position, targetPosition, movementLerp);
        normal = Vector3.Lerp(normal, targetNormal, movementLerp);
    }

    protected override float GetStepDuration() { return manager.JumpSmoothTime; }

    private void UpdateTargets()
    {
        // 0..1 value of how close to the center of the FootRange the foot should be 
        float targetCenterBias;

        // Constantly search for most "suitable" landing spot
        if (PerformRaycasts(DefaultRaycastOrigins(), out RaycastHit hit))
        {
            float hitDistance = Vector3.Distance(hit.point, footRange.GetCenter());

            // Keep track of the farthest found landing distance this step has seen
            if (hitDistance > maxLandDistance) 
                maxLandDistance = hitDistance;

            // Keep track of the latest suitable target position
            // This is so we can smoothly move away from it if player moves
            targetPositionMemory = footRange.GetClosestPointTo(hit.point);
            targetNormalMemory = hit.normal;

            targetCenterBias = MathUtils.Normalize(hitDistance, maxLandDistance, footRange.GetSize());
        }
        else
        {
            maxLandDistance = 0;
            targetCenterBias = 1;
        }
        // Smoothly change the actual bias towards the FootRange center
        centerBias = Mathf.SmoothDamp(centerBias, targetCenterBias, ref centerBiasSmoothVelocity, 0.2f);

        targetPosition = footRange.GetCenter() * centerBias + targetPositionMemory * (1 - centerBias);
        targetNormal = Vector3.up * centerBias + targetNormalMemory * (1 - centerBias);
    }

    protected override bool PerformRaycast(Vector3 origin, out RaycastHit hit)
    {
        Vector3 playerVelocity = manager.GetControllerVelocity();
        // Don't search upwards -- this is so we can anticipate steps even when player is still rising
        float playerVelocityY = Mathf.Min(playerVelocity.y, 0);
        Vector3 direction = new Vector3(playerVelocity.x, playerVelocityY, playerVelocity.z).normalized;

        // Raycast from a bit behind the center to make sure we don't get trapped inside an object
        Vector3 adjustedOrigin = origin - direction * footRange.GetSize();

        Ray ray = new Ray(adjustedOrigin, direction);
        return PerformRaycast(ray, out hit, GetFullRayDistance());
    }

    private float GetFullRayDistance() { return footRange.GetSize() * 4; }

    private bool PerformRaycast(Ray ray, out RaycastHit hit, float distance)
    {
        bool rayHit = Physics.Raycast(ray, out hit, distance, GlobalState.GroundLayerMask);
        return rayHit && footRange.IsWithinSlope(hit.normal);
    }

    public override bool IsComplete()
    {
        // Raycast down to see if we've touched suitable land
        float distance = manager.FootRestDistanceThreshold;
        Vector3 origin = position + Vector3.up * distance / 2f;
        Ray ray = new Ray(origin, Vector3.down);
        bool rayHit = PerformRaycast(ray, out RaycastHit hit, distance);
        return rayHit;
    }

    // Floating step behavior never really "fails". We just keep floating
    public override bool FailedToStep() { return false; }
}