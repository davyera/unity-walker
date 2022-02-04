using System;
using UnityEngine;

public class PhysicsConstants : MonoBehaviour
{
    private static PhysicsConstants singleton;

    public static PhysicsConstants Instance { get { return singleton; } }

    private void Awake()
    {
        if (singleton != null && singleton != this)
            Destroy(this);
        else
            singleton = this;
    }

    public float gravity = -9.81f;
    public float terminalVelocity = -2f;
    public float restingGravityVelocity = -2f;
    public float friction = 0.3f;
    public float drag = 5f;
}
