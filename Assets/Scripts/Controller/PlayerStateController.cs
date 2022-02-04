using System;
using UnityEngine;

public enum PlayerGroundState
{
    GROUNDED,
    SLIDING,
    FLOATING,
    SWIMMING
}

public enum PlayerHeadState
{
    BLOCKED,
    FREE
}

public class PlayerStateController : MonoBehaviour
{
    public event Action OnGrounded;
    public event Action OnUnGrounded;
    public event Action OnJumped;
    public event Action OnSwimming;
    public event Action OnSwimStop;
    public event Action OnSlide;
    public event Action OnHeadBlock;

    private PlayerGroundState groundState;
    private PlayerHeadState headState;

    public bool IsGrounded() { return groundState == PlayerGroundState.GROUNDED; }
    public bool IsSliding() { return groundState == PlayerGroundState.SLIDING; }
    public bool IsFloating() { return groundState == PlayerGroundState.FLOATING; }
    public bool IsSwimming() { return groundState == PlayerGroundState.SWIMMING; }
    public bool IsHeadBlocked() { return headState == PlayerHeadState.BLOCKED; }

    public void SetGroundState(PlayerGroundState newState)
    {
        if (groundState != newState)
        {
            groundState = newState;

            if (IsGrounded())
                OnGrounded?.Invoke();
            else
                OnUnGrounded?.Invoke();

            if (IsSliding())
                OnSlide?.Invoke();
        }
    }

    public void SetHeadState(PlayerHeadState newState)
    {
        if (headState != newState)
        {
            headState = newState;

            if (IsHeadBlocked())
                OnHeadBlock?.Invoke();
        }
    }

    public void TriggerJumped() { OnJumped?.Invoke(); }
    public void TriggerSwimming() { OnSwimming?.Invoke(); }
    public void TriggerSwimStop() { OnSwimStop?.Invoke(); }
}
