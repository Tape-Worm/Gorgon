
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, January 13, 2013 5:59:29 PM
// 

using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// The spray can effect state object
/// </summary>
internal class SprayCan
{
    // Current time.
    private float _time;
    // Maximum time.
    private float _maxTime;
    // Flag to indicate that the spray is active.
    private bool _isActive;
    // Time at which the spray became active.
    private float _activeStartTime;
    // Current alpha.
    private float _alpha;
    private float _maxAlpha;
    // Vibration amount.
    private float _vibAmount;
    private float _maxVibe;
    // Spray amount.
    private float _sprayAmount;
    private float _maxSpray;

    /// <summary>
    /// Property to return the point size for the spray.
    /// </summary>
    public float SprayPointSize
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the origin of the spray.
    /// </summary>
    public Vector2 Origin
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the color of the spray effect.
    /// </summary>
    public GorgonColor SprayColor
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return whether the spray is active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if ((_alpha <= 0.0f) || (_maxTime <= 0.0f))
            {
                _alpha = 0.0f;
                _time = 0.0f;
                _maxTime = 0.0f;
                _vibAmount = 0.0f;
                _sprayAmount = 0.0f;
                _isActive = false;
            }
            else
            {
                _isActive = value;
            }

            _activeStartTime = GorgonTiming.SecondsSinceStart;

            SprayPointSize = 0.0f;

            if (!_isActive)
            {
                _time = 0;
            }
        }
    }

    /// <summary>
    /// Property to set or return the current spray position.
    /// </summary>
    public Vector2 Position
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the amount to spray.
    /// </summary>
    public float Amount
    {
        get => _sprayAmount;
        set
        {
            if (value <= 0.0f)
            {
                IsActive = false;
            }

            _maxSpray = _sprayAmount = value;
        }
    }

    /// <summary>
    /// Property to set or return the time the spray should live.
    /// </summary>
    public float Time
    {
        get => _time;
        set
        {
            if (value <= 0.0f)
            {
                value = 0.0f;
                IsActive = false;
            }

            _maxTime = _time = value;
        }
    }

    /// <summary>
    /// Property to set or return the amount to vibrate the controller during the spray.
    /// </summary>
    public float VibrationAmount
    {
        get => _vibAmount;
        set
        {
            if (value <= 0.0f)
            {
                value = 0.0f;
            }

            _maxVibe = _vibAmount = value;
        }
    }

    /// <summary>
    /// Property to set or return the alpha amount for the spray.
    /// </summary>
    public float SprayAlpha
    {
        get => _alpha;
        set
        {
            if (value <= 0.0f)
            {
                IsActive = false;
                value = 0.0f;
            }

            _maxAlpha = _alpha = value;
        }
    }

    /// <summary>
    /// Function to update the sprayer.
    /// </summary>
    /// <param name="sprayVector">The directional vector for the spray.</param>
    public void Update(Vector2 sprayVector)
    {
        float appTime = GorgonTiming.SecondsSinceStart - _activeStartTime;

        // Get unit time.
        float unitTime = (appTime / _maxTime).Max(0).Min(1);

        float magnitude = (sprayVector.X * sprayVector.X + sprayVector.Y * sprayVector.Y).Sqrt();

        // Decrease spray time.
        _time = _maxTime - appTime;

        // If we're out of time, then leave.
        if (_time <= 0.0f)
        {
            IsActive = false;
            return;
        }

        // Decrease alpha over time.
        _alpha = _maxAlpha * (1.0f - unitTime);
        if (_alpha < 0.0f)
        {
            _alpha = 0.0f;
        }

        // Decrease vibration over time.
        _vibAmount = _maxVibe * (1.0f - unitTime);
        if (_vibAmount < 0.0f)
        {
            _vibAmount = 0.0f;
        }

        // Decrease the amount over time and by throttle pressure.
        _sprayAmount = _maxSpray - (_maxSpray * ((1.0f - unitTime) * magnitude));

        if (_sprayAmount < 0)
        {
            _sprayAmount = 0;
        }

        SprayPointSize = ((1.0f - unitTime) * 36.0f) + 4.0f;

        // Scale the vector.
        sprayVector = new Vector2(_sprayAmount * sprayVector.X, _sprayAmount * sprayVector.Y);

        // Update the spray position.
        Vector2 jitter = new(GorgonRandom.RandomSingle(-100 * unitTime, 100.0f * unitTime), GorgonRandom.RandomSingle(-100.0f * unitTime, 100.0f * unitTime));
        Position = new Vector2(Origin.X + sprayVector.X + jitter.X, Origin.Y + sprayVector.Y + jitter.Y);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SprayCan" /> struct.
    /// </summary>
    /// <param name="spraycColor">The color for this spray can.</param>
    public SprayCan(GorgonColor spraycColor)
    {
        SprayColor = spraycColor;
        Position = Vector2.Zero;
        Amount = 0.0f;
        Time = 0.0f;
        VibrationAmount = 0.0f;
        SprayAlpha = 255.0f;
        IsActive = false;
    }
}
