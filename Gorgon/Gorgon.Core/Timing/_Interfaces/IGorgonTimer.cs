namespace Gorgon.Timing;

/// <summary>
/// Functionality to measure an interval of time
/// </summary>
/// <remarks>
/// <para>
/// Timers can be used to set up events that occur during a predefined time period, and/or can be used to measure the amount of time that operation takes to complete. 
/// </para>
/// <para>
/// A timer is closely tied to the operating system, thus a concrete class will be provided by an operating system specific assembly
/// </para>
/// </remarks>
public interface IGorgonTimer
{
    /// <summary>
    /// Property to return the number of milliseconds elapsed since the timer was started.
    /// </summary>
    double Milliseconds
    {
        get;
    }

    /// <summary>
    /// Property to return the number of microseconds elapsed since the timer was started.
    /// </summary>
    double Microseconds
    {
        get;
    }

    /// <summary>
    /// Property to return the number of seconds elapsed since the timer was started.
    /// </summary>
    double Seconds
    {
        get;
    }

    /// <summary>
    /// Property to return the number of minutes elapsed since the timer was started.
    /// </summary>
    double Minutes
    {
        get;
    }

    /// <summary>
    /// Property to return the number of hours elapsed since the timer was started.
    /// </summary>
    double Hours
    {
        get;
    }

    /// <summary>
    /// Property to return the number of days elapsed since the timer was started.
    /// </summary>
    double Days
    {
        get;
    }

    /// <summary>
    /// Property to return the number of ticks since the timer was started.
    /// </summary>
    long Ticks
    {
        get;
    }

    /// <summary>
    /// Property to return whether this timer has a resolution of less than 1 millisecond or not.
    /// </summary>
    bool IsHighResolution
    {
        get;
    }

    /// <summary>
    /// Function to reset the timer.
    /// </summary>
    void Reset();

}
