namespace Diversions
{
    public enum MarshalOption
    {
        CurrentThread,
        StartNewTask,
        RunTask,
        Dispatcher,
        //TODO: create an option that posts to a Reactive Scheduler
        //Scheduler,
        UserDefined
    }
}
