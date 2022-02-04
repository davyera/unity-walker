using UnityEngine;

public class ArmMovement : FootMovementStrategy
{
    private readonly ArmManager armManager;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private GameObject targetItem = null;
    private float itemAwaitLerp = 0;

    public ArmMovement(ArmManager armManager, FootTarget footTarget)
        : base(footTarget)
    {
        this.armManager = armManager;
        position = GetRestPosition();
        rotation = GetRestRotation();
    }

    public override void Update()
    {
        UpdateLerp();
        UpdateTargetItem();
        CalculateTarget();

        position = Vector3.Lerp(position, targetPosition, itemAwaitLerp);
        rotation = Quaternion.Lerp(rotation, targetRotation, itemAwaitLerp);
    }

    private void UpdateLerp()
    {
        if (itemAwaitLerp < 1)
            itemAwaitLerp += armManager.GetItemAwaitSpeed() * Time.deltaTime;
    }

    private void UpdateTargetItem()
    {
        GameObject closestItem = armManager.GetNearestReachableItem();
        if (closestItem != targetItem)
        {
            // restart our lerp movement
            itemAwaitLerp = 0;
            targetItem = closestItem;
        }
    }

    private void CalculateTarget()
    {
        if (targetItem == null)
            SetTargetToRest();
        else
            SetTargetToAwaitItem();
    }

    private void SetTargetToRest()
    {
        targetPosition = GetRestPosition();
        targetRotation = GetRestRotation();
    }

    private void SetTargetToAwaitItem()
    {
        Vector3 itemVector = targetItem.transform.position - position;
        Vector3 itemDirection = itemVector.normalized;
        float itemDistance = itemVector.magnitude;
        float awaitDistance = Mathf.Min(armManager.GetItemAwaitDistance(), itemDistance);
        targetPosition = GetRestPosition() + itemDirection * awaitDistance;

        Quaternion itemTilt = Quaternion.FromToRotation(footTarget.transform.forward, itemDirection);
        targetRotation = itemTilt * rotation;
    }

    private Vector3 GetRestPosition()
    {
        Transform restCenter = armManager.GetArmRestCenter();
        Vector3 offset = restCenter.right * armManager.GetArmRestWidth() / 2;
        int sign = footTarget.IsRightHandSide() ? 1 : -1;
        return restCenter.position + sign * offset;
    }

    private Quaternion GetRestRotation()
    {
        return armManager.GetArmRestCenter().rotation;
    }
}
