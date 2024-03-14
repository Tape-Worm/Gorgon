
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


using Gorgon.Core;
using Gorgon.Input;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// The spray can effect state object
/// </summary>
internal class SprayCan
{

    private float _time;                    // Current time.
    private float _maxTime;                 // Maximum time.
    private bool _isActive;                 // Flag to indicate that the spray is active.
    private float _activeStartTime;         // Time at which the spray became active.
    private float _alpha;                   // Current alpha.
    private float _maxAlpha;                // Maximum alpha.
    private float _vibAmount;               // Vibration amount.
    private float _vibMax;                  // Maximum vibration.
    private float _sprayAmount;             // Spray amount.
    private float _sprayMax;                // Maximum spray amount.			



    /// <summary>
    /// Property to return the point size for the spray.
    /// </summary>
    public float SprayPointSize
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return whether the spray needs to be reset.
    /// </summary>
    public bool NeedReset
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the index of the controller.
    /// </summary>
    public int Index
    {
        get;
    }

    /// <summary>
    /// Property to return the joystick that owns this spray.
    /// </summary>
    public IGorgonGamingDevice Controller
    {
        get;
    }

    /// <summary>
    /// Property to set or return the origin of the spray.
    /// </summary>
    public PointF Origin
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the color of the spray effect.
    /// </summary>
    public Color SprayColor
    {
        get
        {
            int colorValue = (int)(((uint)0xFF << (Index * 8)) | ((uint)SprayAlpha << 24));

            return Color.FromArgb(colorValue);
        }
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
                _maxAlpha = 0.0f;
                _time = 0.0f;
                _maxTime = 0.0f;
                _vibAmount = 0.0f;
                _vibMax = 0.0f;
                _sprayAmount = 0.0f;
                _sprayMax = 0.0f;
                _isActive = false;
            }
            else
            {
                _isActive = value;
                NeedReset = false;
                _activeStartTime = GorgonTiming.SecondsSinceStart;
            }

            SprayPointSize = 0.0f;

            if (!_isActive)
            {
                StopVibration();
            }
        }
    }

    /// <summary>
    /// Property to set or return the current spray position.
    /// </summary>
    public PointF Position
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
            else
            {
                _sprayAmount = value;
                _sprayMax = _sprayAmount;
            }
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

            _time = value;
            _maxTime = _time;
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
                _vibAmount = 0.0f;
                return;
            }

            _vibAmount = value;
            _vibMax = value;
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

            _alpha = value;
            _maxAlpha = _alpha;
        }
    }



    /// <summary>
    /// Function to update the sprayer.
    /// </summary>
    public void Update()
    {
        float throttleValue = Controller.Axis[GamingDeviceAxis.RightTrigger].Value;
        float appTime = GorgonTiming.SecondsSinceStart - _activeStartTime;

        // Get unit time.
        float unitTime = appTime / _maxTime;

        // Vibrate our controller.
        Controller.Vibrate(1, (int)_vibAmount);

        // Get the spray vector in a -1 .. 1 range.
        var sprayVector = new PointF(Controller.Axis[GamingDeviceAxis.RightStickX].Value - Controller.Info.AxisInfo[GamingDeviceAxis.RightStickX].Range.Minimum,
                                    Controller.Axis[GamingDeviceAxis.RightStickY].Value - Controller.Info.AxisInfo[GamingDeviceAxis.RightStickY].Range.Minimum);

        sprayVector = new PointF(((sprayVector.X / (Controller.Info.AxisInfo[GamingDeviceAxis.RightStickX].Range.Range + 1)) * 2.0f) - 1.0f,
                                 -(((sprayVector.Y / (Controller.Info.AxisInfo[GamingDeviceAxis.RightStickY].Range.Range + 1)) * 2.0f) - 1.0f));

        // Calculate angle without magnitude.
        var sprayVectorDelta = new PointF(sprayVector.X, sprayVector.Y);
        float sprayAngle = 0.0f;

        // Ensure that we get the correct angle.
        if (sprayVectorDelta.Y.EqualsEpsilon(0.0f))
        {
            if (sprayVectorDelta.X < 0.0f)
            {
                sprayAngle = (-90.0f).ToRadians();
            }
            else if (sprayVectorDelta.X > 0.0f)
            {
                sprayAngle = (90.0f).ToRadians();
            }
        }
        else
        {
            sprayAngle = (sprayVectorDelta.Y).ATan(sprayVectorDelta.X) + (-45.0f).ToRadians();
        }

        // Get sine and cosine for the angle.
        float sin = sprayAngle.Sin();
        float cos = sprayAngle.Cos();

        // Decrease spray time.
        _time = _maxTime - appTime;

        // If we're out of time, then leave.
        if (_time <= 0.0f)
        {
            IsActive = false;
            NeedReset = true;
            return;
        }

        // Decrease alpha over time.
        _alpha = _maxAlpha - (_maxAlpha * unitTime);
        if (_alpha < 0.0f)
        {
            _alpha = 0.0f;
        }

        // Decrease vibration over time.
        _vibAmount = _vibMax - (_vibMax * unitTime);
        if (_vibAmount < 0.0f)
        {
            _vibAmount = 0.0f;
        }

        // Decrease the amount over time and by throttle pressure.
        _sprayAmount = _sprayMax * unitTime;

        SprayPointSize = (unitTime * 30.0f) + 10.0f;

        // Scale the vector.
        sprayVector = new PointF(_sprayAmount * (cos - sin), _sprayAmount * (sin + cos));

        // Update the spray position.
        var jitter = new PointF(GorgonRandom.RandomSingle(-_sprayAmount / _sprayMax * throttleValue / 10.0f, _sprayAmount / _sprayMax * throttleValue / 10.0f),
                                    GorgonRandom.RandomSingle(-_sprayAmount / _sprayMax * throttleValue / 10.0f, _sprayAmount / _sprayMax * throttleValue / 10.0f));
        Position = new PointF(Origin.X + sprayVector.X + jitter.X, Origin.Y + sprayVector.Y + jitter.Y);
    }

    /// <summary>
    /// Function to force vibration to stop.
    /// </summary>
    public void StopVibration()
    {
        if (Controller.IsConnected)
        {
            Controller.Vibrate(1, 0);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SprayCan" /> struct.
    /// </summary>
    /// <param name="controller">The joystick that owns this spray.</param>
    /// <param name="controllerIndex">Index of the controller.</param>
    public SprayCan(IGorgonGamingDevice controller, int controllerIndex)
    {
        Index = controllerIndex;
        Controller = controller;
        Position = PointF.Empty;
        Amount = 0.0f;
        Time = 0.0f;
        VibrationAmount = 0.0f;
        SprayAlpha = 255.0f;
        IsActive = false;
    }

}
