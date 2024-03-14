using DX = SharpDX;

namespace Gorgon.Editor.Rendering;

/// <summary>
/// Draws a marching ants effect for a rectangle.
/// </summary>
public interface IMarchingAnts
    : IDisposable
{
    /// <summary>
    /// Function to animate the marching ants.
    /// </summary>
    void Animate();

    /// <summary>
    /// Function to draw the marching ants rectangle.
    /// </summary>
    /// <param name="rect">The rectangular region to draw in.</param>
    void Draw(DX.RectangleF rect);
}
