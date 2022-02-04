using System.Collections.Generic;
using UnityEngine;

public class ArmManager : FootMovementManager
{
    [SerializeField]
    private TriggerCollider itemCollider;
    [SerializeField]
    private Transform armRestCenter;

    [SerializeField]
    private float armItemAwaitSpeed = 2f;
    [SerializeField]
    private float armItemGrabSpeed = 20f;
    [SerializeField]
    private float itemReachAngle = 45f;

    private float itemAwaitDistance;
    private float armRestWidth;

    private List<GameObject> nearbyItems = new List<GameObject>();

    // TODO: If there is flickering between 2 objects, may need to add a TimeoutCounter for switching items
    private GameObject nearestReachableItem = null;

    public void Start()
    {
        SetAttributes();

        itemCollider.OnEnter += HandleNearbyItem;
        itemCollider.OnExit += HandleItemExit;
    }

    private void SetAttributes()
    {
        armRestWidth = Manager.GetBoundRadius();
        itemAwaitDistance = Manager.GetBoundRadius() / 2;
    }

    private void HandleNearbyItem(GameObject item) { nearbyItems.Add(item); }
    private void HandleItemExit(GameObject item) { nearbyItems.Remove(item); }

    private Vector3 GetCenter() { return armRestCenter.position; }

    public float GetItemAwaitDistance() { return itemAwaitDistance; }
    public float GetArmRestWidth() { return armRestWidth; }
    public float GetItemAwaitSpeed() { return armItemAwaitSpeed; }
    public Transform GetArmRestCenter() { return armRestCenter; }
    public GameObject GetNearestReachableItem() { return nearestReachableItem; }

    private void Update()
    {
        UpdateNearestReachableItem();
    }

    private void UpdateNearestReachableItem()
    {
        GameObject closestItem = null;
        float closestItemDistance = float.MaxValue;

        foreach (GameObject item in nearbyItems) {
            float itemDistance = Vector3.Distance(GetCenter(), item.transform.position);

            // Ensure the object is not occluded by raycasting
            if (itemDistance < closestItemDistance && IsItemReachable(item))
            {
                closestItem = item;
                closestItemDistance = itemDistance;
            }
        }
        nearestReachableItem = closestItem;
    }

    private bool IsItemReachable(GameObject item)
    {
        Vector3 center = GetCenter();
        Vector3 direction = item.transform.position - center;
        Vector3 forward = Manager.transform.forward;

        // Check if item is within the "reaching" angle
        if (Vector3.Angle(forward, direction) > itemReachAngle)
            return false;

        // Raycast to ensure item isn't occluded by another obstacle
        Ray ray = new Ray(center, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude))
        {
            return hit.transform.gameObject.layer == GlobalState.ItemLayer;
        }
        return false;
    }
}
