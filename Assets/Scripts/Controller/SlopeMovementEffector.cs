using UnityEngine;

public class SlopeMovementEffector : MonoBehaviour
{
    private PlayerStateController state;
    private WalkMovementComponent movement;
    private BoundsChecker bounds;

    [SerializeField]
    private float slopeVelocityAngleThreshold = 15;
    private float slopeMaxLimit;

    private void Start()
    {
        state = GetComponent<PlayerStateController>();
        movement = GetComponent<WalkMovementComponent>();
        bounds = GetComponent<BoundsChecker>();
        UpdateAttributes();
    }

    private void UpdateAttributes()
    {
        slopeMaxLimit = GetComponent<CharacterController>().slopeLimit;
    }

    public float Get()
    {
        if (state.IsGrounded())
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, movement.GetVelocity().normalized);

            float slopeAngle = Mathf.Clamp(Vector3.SignedAngle(Vector3.up, bounds.GetGroundNormal(), rotationAxis), -slopeMaxLimit, slopeMaxLimit);

            // If we're within the slope threshold (+ or -), keep the velocity effect at 1
            if (Mathf.Abs(slopeAngle) < slopeVelocityAngleThreshold)
                return 1;
            // If we're negative (going uphill), velocity effect will range from 0.5 to 1 (if we hit the slope limit)
            else if (slopeAngle < -slopeVelocityAngleThreshold)
                return 0.5f + MathUtils.Normalize(slopeAngle, -slopeVelocityAngleThreshold, -slopeMaxLimit) / 2;
            // If we're positive (going downhill), velocity effect will increase from 1 to 1.5 (if we hit slope limit)
            else if (slopeAngle > slopeVelocityAngleThreshold)
                return 1 + MathUtils.Normalize(slopeAngle, slopeVelocityAngleThreshold, slopeMaxLimit) / 2;
        }
        return 1;
    }
}
