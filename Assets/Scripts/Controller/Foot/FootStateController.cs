
public enum FootState
{
    FLOATING,
    STEPPING,
    PLANTED
}

public class FootStateController
{
    private readonly TimeoutCounter stateChangeTimeout = new TimeoutCounter(0.1f);
    private FootState state;

    public FootStateController()
    {
        state = FootState.PLANTED;
    }

    // Ignores timeout
    public void ForceSetState(FootState newState)
    {
        if (state != newState)
        {
            state = newState;
            stateChangeTimeout.StartCounter();
        }
    }

    public void SetState(FootState newState)
    {
        if (!stateChangeTimeout.IsTimedOut())
            ForceSetState(newState);
    }

    public bool IsStepping() { return state == FootState.STEPPING; }
    public bool IsFloating() { return state == FootState.FLOATING; }
    public bool IsPlanted() { return state == FootState.PLANTED; }
}
