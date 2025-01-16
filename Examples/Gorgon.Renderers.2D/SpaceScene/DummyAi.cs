using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// A very dumb AI that does nothing but increase the engine burn after 10 seconds and slow down and stop after 40
/// </summary>
internal class DummyAi
{
    /// <summary>
    /// Property to set or return the ship to control.
    /// </summary>
    /// <remarks>
    /// In a real app, a more abstract object would be better suited so you can attach your AI to a multitude of object types.
    /// </remarks>
    public Ship Ship
    {
        get;
        set;
    }

    /// <summary>
    /// Function to perform an update from the so called "AI".
    /// </summary>
    public void Update()
    {
        if ((GorgonTiming.SecondsSinceStart < 10) || (Ship is null))
        {
            return;
        }

        if (GorgonTiming.SecondsSinceStart < 40)
        {
            Ship.Accelerate();
        }
        else if (Ship.Speed > 0)
        {
            Ship.Decelerate();
        }
        else
        {
            Ship.HardStop();
        }
    }
}
