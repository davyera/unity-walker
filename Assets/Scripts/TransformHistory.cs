using UnityEngine;

/** 
 * TODO: performance improvements:
 *          1. Adjustable sampling rate: probably don't need to record position on every frame
 *          2. Periodically clear out animation history (create new VectorHistory objects to replace)
 */
public class TransformHistory : MonoBehaviour
{
    [SerializeField]
    private Transform trackedTransform;
    [SerializeField]
    private float lookbackTime = 0.2f;

    private VectorHistory positionHistory;
    private VectorHistory rotationHistory;

    private Vector3 latestPositionDelta;
    private Vector3 latestRotationDelta;

    public Vector3 GetPositionDelta() { return latestPositionDelta; }
    public Vector3 GetRotationDelta() { return latestRotationDelta; }

    public void StartTracking() { InitHistory(); }
    public void StopTracking() { ClearHistory(); }

    private void InitHistory()
    {
        if (!IsTracking())
        {
            positionHistory = new VectorHistory();
            rotationHistory = new VectorHistory();
        }
    }

    private void ClearHistory()
    {
        positionHistory = null;
        rotationHistory = null;
    }

    private bool IsTracking() { return positionHistory != null && rotationHistory != null; }

    private void Update()
    {
        if (IsTracking())
        {
            positionHistory.AddPoint(trackedTransform.position);
            rotationHistory.AddPoint(trackedTransform.eulerAngles);
            UpdateDeltas();
        }
    }

    private Vector3 GetCurrentPosition() { return trackedTransform.position; }
    private Vector3 GetCurrentRotation() { return trackedTransform.eulerAngles; }

    private void UpdateDeltas()
    {
        float targetTime = Time.time - lookbackTime;
        latestPositionDelta = GetCurrentPosition() - positionHistory.GetValue(targetTime);
        latestRotationDelta = GetCurrentRotation() - rotationHistory.GetValue(targetTime);
    }
}
