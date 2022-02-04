using UnityEngine;

public class FootWalkManager : FootMovementManager
{
    [SerializeField]
    private float stepHeightRatio = 0.05f;

    private FootRange rightLegFootRange;
    private FootRange leftLegFootRange;

    // FootRange sizing properties
    private float rangeSize;
    private float rangeMin;
    private float rangeMax;
    private float rangeWidth;

    // Smoothing range changes
    private float rangeSizeSmoothVelocity;
    [SerializeField]
    private float rangeSmoothTime = 0.5f;
    public float RangeSmoothTime { get => rangeSmoothTime; }
    [SerializeField]
    private float maxRangeOffsetMultiplier = 0.5f;
    private Vector3 rangeOffset;
    private Vector3 rangeOffsetSmoothVelocity;

    private float stepHeight;
    private float raycastRange;

    // How much faster the step will move in relation to the player's speed
    [SerializeField]
    private float stepSpeedMultiplier = 2f;

    // How much "cushion" we give the foot to be considered at rest in relation to the center of its FootRange
    [SerializeField]
    private float footRestDistanceThreshold = 0.05f;
    public float FootRestDistanceThreshold { get => footRestDistanceThreshold; }

    // Defines default FootRange radius as a multiple of the player's bounds radius
    [SerializeField]
    private float rangeMultiplier = 1.5f;

    // How long (in seconds) it will take for feet to drift to their rest position when "floating"
    [SerializeField]
    private float floatDriftTime = 0.5f;

    // Arc of foot movement when stepping
    [SerializeField]
    private AnimationCurve stepArcCurve;
    public AnimationCurve StepArcCurve { get => stepArcCurve; }

    // Arc of foot movement when jumping
    [SerializeField]
    private AnimationCurve jumpCurve;
    public AnimationCurve JumpCurve { get => jumpCurve; }

    // How long to smooth jump movement
    [SerializeField]
    private float jumpSmoothTime = 0.5f;
    public float JumpSmoothTime { get => jumpSmoothTime; }

    // Timeout preventing feet from trying to issue a new step when character jumps
    private TimeoutCounter jumpTimeout;

    // Amount of speed to apply to player controller when feet are "unbalanced" (1 or more is floating)
    [SerializeField]
    private float unbalancedSpeed = 25f;
    public float UnbalancedSpeed { get => unbalancedSpeed; }

    private void Start()
    {
        jumpTimeout = new TimeoutCounter(1f);

        UpdateAttributes();
        InitLegFootRanges();

        Manager.player.state.OnJumped += HandleJumped;
        Manager.player.state.OnGrounded += HandleGrounded;
    }

    private void UpdateAttributes()
    {
        rangeMin = Manager.GetBoundRadius() * rangeMultiplier;
        rangeMax = rangeMin * 1.5f;
        rangeSize = rangeMin;
        rangeWidth = rangeMax - rangeMin;
        raycastRange = rangeMax * 2;

        stepHeight = Manager.player.controller.height * stepHeightRatio;
    }

    private void InitLegFootRanges()
    {
        if (rightLegFootRange != null && leftLegFootRange != null) return;
        Vector3 groundPosition = Manager.player.GetControllerGroundPosition();
        Vector3 footWidth = transform.right * Manager.GetBoundRadius();
        rightLegFootRange = new FootRange(this, groundPosition + footWidth);
        leftLegFootRange = new FootRange(this, groundPosition - footWidth);
    }

    public bool IsJumpTimedOut() { return jumpTimeout.IsTimedOut(); }
    private void HandleJumped() { jumpTimeout.StartCounter(); }
    private void HandleGrounded() { jumpTimeout.EndCounter(); }

    private void FixedUpdate()
    {
        UpdateStepRadius();    
    }

    private void UpdateStepRadius()
    {
        Vector3 latestVelocity = GetControllerVelocity();
        float latestSpeed = latestVelocity.magnitude;
        float rangeMultiplier = MathUtils.Normalize(latestSpeed, Manager.GetMaxSpeed(), 0);

        float targetRangeSize = rangeMin + rangeWidth * rangeMultiplier;
        rangeSize = Mathf.SmoothDamp(rangeSize, targetRangeSize, ref rangeSizeSmoothVelocity, rangeSmoothTime);

        Vector3 targetRangeOffset = latestVelocity.normalized * rangeMultiplier * Manager.GetBoundRadius() * maxRangeOffsetMultiplier;
        rangeOffset = Vector3.SmoothDamp(rangeOffset, targetRangeOffset, ref rangeOffsetSmoothVelocity, rangeSmoothTime);
    }

    public FootRange GetLeftLegFootRange()
    {
        if (leftLegFootRange == null) InitLegFootRanges();
        return leftLegFootRange;
    }

    public FootRange GetRightLegFootRange()
    {
        if (rightLegFootRange == null) InitLegFootRanges();
        return rightLegFootRange;
    }

    private float GetMinStepSpeed() { return Manager.GetMaxSpeed() / 4; }
    public float GetRangeSize() { return rangeSize; }
    public float GetRaycastRange() { return raycastRange; }
    public Vector3 GetControllerVelocity() { return Manager.player.controller.velocity; }
    public Quaternion GetParentRotation() { return Manager.GetParentRotation(); }

    public float GetStepSpeed()
    {
        float targetSpeed = stepSpeedMultiplier * GetControllerVelocity().magnitude;
        return Mathf.Max(targetSpeed, GetMinStepSpeed());
    }

    public float GetStepHeight()
    {
        float velocityMultiplier = MathUtils.Normalize(GetControllerVelocity().magnitude, Manager.GetMaxSpeed(), 0.1f);
        return stepHeight * velocityMultiplier;
    }

    public Vector3 GetStepSize()
    {
        float stepSize = rangeSize * 0.95f; // Allow some buffer so we don't over-step the range bounds
        return GetControllerVelocity().normalized * stepSize;
    }

    public Vector3 GetRangeCenterOffset() { return rangeOffset; }

    public bool IsSlopeTraversable(Vector3 normal)
    {
        return Vector3.Angle(Vector3.up, normal) < Manager.player.controller.slopeLimit;
    }

    public bool IsPlayerGrounded() { return Manager.player.state.IsGrounded(); }
    public Vector3 GetPlayerGroundedPosition() { return Manager.player.GetGroundedPosition(); }

    private void OnDrawGizmos()
    {
        if (rightLegFootRange != null && leftLegFootRange != null)
        {
            Gizmos.DrawWireSphere(rightLegFootRange.GetCenter(), rightLegFootRange.GetSize());
            Gizmos.DrawWireSphere(leftLegFootRange.GetCenter(), leftLegFootRange.GetSize());
        }
    }
}
