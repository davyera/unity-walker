using System;
using UnityEngine;

public class UpdateMaster : MonoBehaviour
{
    private static UpdateMaster singleton;

    public static void RegisterUpdateable(Updateable updateable)
    {
        InitSingleton();
        singleton.OnUpdate += updateable.Update;
        singleton.OnFixedUpdate += updateable.FixedUpdate;
    }

    public static void DeregisterUpdateable(Updateable updateable)
    {
        singleton.OnUpdate -= updateable.Update;
        singleton.OnFixedUpdate -= updateable.FixedUpdate;
    }

    private static UpdateMaster InitSingleton()
    {
        if (singleton == null)
            singleton = new GameObject("[Update Master]").AddComponent<UpdateMaster>();
        return singleton;
    }

    private Action OnUpdate;
    private Action OnFixedUpdate;

    void Update()
    {
        OnUpdate?.Invoke();
    }

    void FixedUpdate()
    {
        OnFixedUpdate?.Invoke();    
    }
}

public class Updateable
{
    private bool registered;

    public Updateable()
    {
        Register();
    }

    protected void Register()
    {
        if (!registered)
        {
            registered = true;
            UpdateMaster.RegisterUpdateable(this);
        }
    }

    protected void Deregister()
    {
        if (registered)
        {
            registered = false;
            UpdateMaster.DeregisterUpdateable(this);
        }
    }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
