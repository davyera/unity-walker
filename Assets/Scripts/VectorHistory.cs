using UnityEngine;

public class VectorHistory
{
    private float startTime = -1;
    private readonly AnimationCurve x = new AnimationCurve();
    private readonly AnimationCurve y = new AnimationCurve();
    private readonly AnimationCurve z = new AnimationCurve();

    public Vector3 GetValue(float atTime)
    {
        float t = atTime < startTime ? startTime : atTime;
        return new Vector3(x.Evaluate(t), y.Evaluate(t), z.Evaluate(t));
    }

    public void AddPoint(Vector3 point)
    {
        float time = Time.time;
        if (startTime == -1) startTime = time;
        x.AddKey(time, point.x);
        y.AddKey(time, point.y);
        z.AddKey(time, point.z);
    }
}
