using System.Collections.Generic;
using UnityEngine;

public class BoundsChecker : MonoBehaviour  
{
    private PlayerStateController state;

    private Transform groundCheck;
    private Transform headCheck;

    private float raycastDistance;
    private float boundCheckRadius;

    private Vector3 groundPosition;
    private Vector3 groundNormal;
    private Vector3 groundNormalChangeVelocity;
    private float groundNormalChangeTime = 0.1f;

    private float slopeLimit;

    private void Start()
    {
        state = GetComponent<PlayerStateController>();
        groundCheck = new GameObject("[GroundCheck]").transform;
        groundCheck.parent = transform;

        headCheck = new GameObject("[HeadCheck]").transform;
        headCheck.parent = transform;

        UpdateBounds();
    }

    private void UpdateBounds()
    {
        PlayerController player = GetComponent<PlayerController>();
        CharacterController controller = GetComponent<CharacterController>();
        float boundsCheckY = player.GetDistanceToGround() - controller.radius;
        Vector3 boundsCheckDistance = new Vector3(0, boundsCheckY, 0);
        groundCheck.position = transform.position - boundsCheckDistance;
        headCheck.position = transform.position + boundsCheckDistance;

        boundCheckRadius = controller.radius;
        raycastDistance = controller.skinWidth * 2;

        slopeLimit = controller.slopeLimit;
    }

    public Vector3 GetGroundPosition() { return groundPosition; }
    public Vector3 GetGroundNormal() { return groundNormal; }

    private void FixedUpdate()
    {
        UpdateGroundBounds();
        UpdateHeadBounds();
    }

    private void UpdateGroundBounds()
    {
        // Use ground check object to check if we are grounded for jumping / physics velocity
        RaycastHit[] hits = CheckBoundHits(groundCheck.position, -groundCheck.up);
        if (hits.Length > 0)
        {
            // Separate "grounded" hits from "slide" hits for later processing
            List<RaycastHit> groundedHits = new List<RaycastHit>();
            List<RaycastHit> slideHits = new List<RaycastHit>();

            foreach (RaycastHit hit in hits)
            {
                // On edge cases, sphere cast returns wonky normals
                // so do another simple raycast a bit above the hit point downwards to derive the "true" normal
                float rayDistance = 0.2f;
                Ray recastRay = new Ray(hit.point + new Vector3(0, rayDistance / 2, 0), Vector3.down);
                RaycastHit recastHit;
                if (Physics.Raycast(recastRay, out recastHit, rayDistance, GlobalState.GroundLayerMask))
                    // If hit normal angle is less than our slope limit, we are "grounded"
                    if (Vector3.Angle(recastHit.normal, Vector3.up) < slopeLimit)
                        groundedHits.Add(recastHit);
                    // Otherwise, we are on a "slide"
                    else
                        slideHits.Add(recastHit);
            }

            // If we got at least one "grounded" hit, consider us fully grounded
            if (groundedHits.Count > 0)
                UpdateGround(groundedHits);
            // Otherwise, use the "slide" hits
            else if (slideHits.Count > 0)
                UpdateGround(slideHits);
        }
        else
            // If there were no hits, don't damp the normal change -- immediately set to 0 so we are no longer "grounded"
            ResetGround();

        // Set state depending on ground
        PlayerGroundState groundState;
        if (groundNormal == Vector3.zero)
            groundState = state.IsSwimming() ? PlayerGroundState.SWIMMING : PlayerGroundState.FLOATING;
        else if (Vector3.Angle(groundNormal, Vector3.up) > slopeLimit)
            groundState = PlayerGroundState.SLIDING;
        else
            groundState = PlayerGroundState.GROUNDED;

        state.SetGroundState(groundState);
    }

    private void UpdateHeadBounds()
    {
        RaycastHit[] headHits = CheckBoundHits(headCheck.position, headCheck.up);
        PlayerHeadState headState = headHits.Length > 0 ? PlayerHeadState.BLOCKED : PlayerHeadState.FREE;

        state.SetHeadState(headState);
    }

    private RaycastHit[] CheckBoundHits(Vector3 origin, Vector3 direction)
    {
        return Physics.SphereCastAll(origin, boundCheckRadius, direction, raycastDistance, GlobalState.BarrierMask);
    }

    // Note: Will smoothly interpolate the normal
    private void UpdateGround(List<RaycastHit> hits)
    {
        Vector3 groundPosition = Vector3.zero;
        Vector3 normalTarget = Vector3.zero;
        if (hits.Count > 0)
        {
            Vector3 totalPositions = Vector3.zero;
            Vector3 totalNormals = Vector3.zero;
            foreach (RaycastHit hit in hits)
            {
                totalPositions += hit.point;
                totalNormals += hit.normal;
            }
            groundPosition = totalPositions / hits.Count;
            normalTarget = totalNormals / hits.Count;
        }
        Vector3 newGroundNormal = Vector3.SmoothDamp(groundNormal, normalTarget, ref groundNormalChangeVelocity, groundNormalChangeTime);
        SetGroundPosition(groundPosition);
        SetGroundNormal(newGroundNormal.normalized);
    }

    private void ResetGround()
    {
        SetGroundPosition(groundCheck.transform.position);
        SetGroundNormal(Vector3.zero);
    }

    private void SetGroundPosition(Vector3 newGroundPosition)
    {
        groundPosition = newGroundPosition;
    }

    private void SetGroundNormal(Vector3 newGroundNormal)
    {
        groundNormal = newGroundNormal;
    }
}
