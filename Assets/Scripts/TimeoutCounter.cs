using UnityEngine;

public class TimeoutCounter : Updateable
{
    private float counter = 0;
    private readonly float timeout;

    public TimeoutCounter(float timeout)
    {
        this.timeout = timeout;
    }

    public override void Update()
    {
        if (IsTimedOut()) counter -= Time.deltaTime;
    }

    public void StartCounter() { counter = timeout; }
    public void EndCounter() { counter = 0; }
    public bool IsTimedOut() { return counter > 0; }
}
