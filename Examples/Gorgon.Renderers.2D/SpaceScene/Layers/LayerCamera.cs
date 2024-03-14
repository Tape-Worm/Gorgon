
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 13, 2021 3:59:57 PM
// 


using System.Numerics;

namespace Gorgon.Examples;

/// <summary>
/// The camera controller for the layers
/// </summary>
/// <remarks>
/// This updates the view point of the user by offsetting all the layers by a specified amount
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="LayerCamera"/> class.</remarks>
/// <param name="layers">The layers used by the application.</param>
internal class LayerCamera(IEnumerable<Layer> layers)
{

    // The layer to track.
    private Layer _trackingLayer;



    /// <summary>
    /// Property to return the list of layers to control.
    /// </summary>
    public IReadOnlyList<Layer> Layers
    {
        get;
    } = layers.ToArray();

    /// <summary>
    /// Property to set or return the layer that will be tracked by the controller.
    /// </summary>
    /// <remarks>
    /// The tracked layer will not have its offset updated.
    /// </remarks>
    public Layer TrackingLayer
    {
        get => _trackingLayer;
        set
        {
            _trackingLayer = value;

            if (_trackingLayer is not null)
            {
                _trackingLayer.Offset = Vector2.Zero;
            }
        }
    }



    /// <summary>
    /// Function to set the position of all unlocked layers.
    /// </summary>
    /// <param name="position">The position to assign to the layers.</param>
    public void SetPosition(Vector2 position)
    {
        TrackingLayer?.Update();

        for (int i = 0; i < Layers.Count; ++i)
        {
            Layer layer = Layers[i];
            if (layer == TrackingLayer)
            {
                continue;
            }

            layer.Offset = position;
            layer.Update();
        }
    }


}
