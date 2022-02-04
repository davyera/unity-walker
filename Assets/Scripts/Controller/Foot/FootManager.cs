using UnityEngine;

public class FootManager : MonoBehaviour
{
    public PlayerController player;

    // designated "arm" foot targets
    public FootTarget rightArmFootTarget;
    public FootTarget leftArmFootTarget;

    // designated "walking" leg foot targets
    public FootTarget rightLegFootTarget;
    public FootTarget leftLegFootTarget;

    // Other foot targets
    public FootTarget rightFrontFootTarget;
    public FootTarget leftFrontFootTarget;
    public FootTarget rightRearFootTarget;
    public FootTarget leftRearFootTarget;

    private ArmManager armManager;
    private FootWalkManager walkManager;
    private IdleFootManager idleManager;

    private FootTarget[] footTargets;

    private void Start()
    {
        armManager = GetComponent<ArmManager>();
        walkManager = GetComponent<FootWalkManager>();
        idleManager = GetComponent<IdleFootManager>();

        InitFootTargets();

        AssignWalking();
        AssignArms();
        AssignIdle();
    }

    private void InitFootTargets()
    {
        rightArmFootTarget.Init(leftArmFootTarget, 22.5f);
        leftArmFootTarget.Init(rightArmFootTarget, -22.5f);

        rightFrontFootTarget.Init(leftFrontFootTarget, 67.5f);
        leftFrontFootTarget.Init(rightFrontFootTarget, -67.5f);

        rightLegFootTarget.Init(leftLegFootTarget, 112.5f);
        leftLegFootTarget.Init(rightLegFootTarget, -112.5f);

        rightRearFootTarget.Init(leftRearFootTarget, 157.5f);
        leftRearFootTarget.Init(rightRearFootTarget, -157.5f);

        footTargets = new FootTarget[]
        {
            rightArmFootTarget,
            leftArmFootTarget,
            rightFrontFootTarget,
            leftFrontFootTarget,
            rightLegFootTarget,
            leftLegFootTarget,
            rightRearFootTarget,
            leftRearFootTarget
        };
    }

    private void AssignArms()
    {
        ArmMovement leftArmMovement = new ArmMovement(armManager, leftArmFootTarget);
        leftArmFootTarget.InitMovementStrategy(leftArmMovement);

        ArmMovement rightArmMovement = new ArmMovement(armManager, rightArmFootTarget);
        rightArmFootTarget.InitMovementStrategy(rightArmMovement);
    }

    private void AssignWalking()
    {
        WalkingMovement leftMovement = new WalkingMovement(walkManager, leftLegFootTarget, walkManager.GetLeftLegFootRange());
        leftLegFootTarget.InitMovementStrategy(leftMovement);

        WalkingMovement rightMovement = new WalkingMovement(walkManager, rightLegFootTarget, walkManager.GetRightLegFootRange());
        rightLegFootTarget.InitMovementStrategy(rightMovement);
    }

    private void AssignIdle()
    {
        AssignIdle(rightFrontFootTarget, leftFrontFootTarget, rightRearFootTarget, leftRearFootTarget);
    }

    private void AssignIdle(params FootTarget[] footTargets)
    {
        foreach (FootTarget footTarget in footTargets)
        {
            IdleMovement idleMovement = GetDefaultMovement(footTarget);
            footTarget.InitMovementStrategy(idleMovement);
        }
    }

    private IdleMovement GetDefaultMovement(FootTarget footTarget)
    {
        return new IdleMovement(idleManager, footTarget);
    }

    public float GetBoundRadius() { return player.controller.radius; }
    public float GetMaxSpeed() { return player.GetMaxSpeed(); }
    public Vector3 GetControllerVelocity() { return player.controller.velocity; }

    public Quaternion GetParentRotation() { return player.transform.rotation; }

    public Vector3 GetFootVelocityComponent()
    {
        Vector3 velocity = Vector3.zero;
        foreach (FootTarget footTarget in footTargets)
        {
            velocity += footTarget.GetPlayerVelocityEffect();
        }
        return velocity;
    }
}
