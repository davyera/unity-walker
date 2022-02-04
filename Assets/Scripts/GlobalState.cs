using System;
using UnityEngine;

public sealed class GlobalState
{
    private GlobalState()
    {
        throw new InvalidOperationException();
    }

    public static int GroundLayer = 8;
    public static LayerMask GroundLayerMask = 1 << GroundLayer;

    public static int ItemLayer = 9;
    public static LayerMask ItemLayerMask = 1 << ItemLayer;

    public static LayerMask BarrierMask = ItemLayerMask | GroundLayerMask;

}
