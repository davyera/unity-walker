using UnityEngine;


public abstract class FootMovementManager : MonoBehaviour
{
    private FootManager manager;
    public FootManager Manager { get => GetManager(); }

    private FootManager GetManager()
    {
        if (manager == null)
            manager = GetComponent<FootManager>();
        return manager;
    }
}
