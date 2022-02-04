using System;
using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    public Action<GameObject> OnEnter;
    public Action<GameObject> OnExit;

    [SerializeField]
    private LayerMask layerMask;

    public void Init() 
    {
        OnEnter += DummyCallback;
        OnExit += DummyCallback;
    }

    private void DummyCallback(GameObject obj) { }

    private void OnTriggerEnter(Collider collider)
    {
        if (MatchesLayer(collider))
            OnEnter?.Invoke(collider.gameObject);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (MatchesLayer(collider))
            OnExit?.Invoke(collider.gameObject);
    }

    private bool MatchesLayer(Collider collider)
    {
        return MathUtils.IsInLayerMask(collider.gameObject, layerMask);
    }
}
