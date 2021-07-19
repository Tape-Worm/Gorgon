#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: May 24, 2019 3:33:44 PM
// 
#endregion

using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Input;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Examples
{
    /// <summary>
    /// A space ship object.
    /// </summary>
    /// <remarks>
    /// This is our space ship that we'll fly around the scene. It's a stand alone object in that it doesn't care about the rendering system powering the scene. It just knows 
    /// that it's a space ship and nothing more than that.
    /// 
    /// Internally it holds our sprite entities that we use to compose our spaceship and it will apply the necessary positioning adjustments to those entities.
    /// 
    /// It can have the layer camera controller attached so that user input can affect the scene.  By making the controller an attachable object, we can give camera control to 
    /// other objects so we can get a different point of view.
    /// 
    /// We also have an "AI" attachable object. This allows us to use an object to set up some actions based on inputs to allow the computer to control the ship.
    /// </remarks>
    internal class Ship
    {
        #region Variables.
        // The ship sprite.
        private SpriteEntity _ship;
        // The engine sprite.
        private SpriteEntity _engine;
        // The layer containing the sprite(s) for this ship.
        private readonly SpritesLayer _layer;
        // The controller for the layers.
        private LayerCamera _layerController;
        // The angle of rotation.
        private float _angle;
        // The controller for the engine animation.
        private readonly GorgonSpriteAnimationController _engineAnimationController;
        // Flag to indicate that we are slowing down.
        private bool _isSlowing;
        // Are we moving backwards?
        private bool _backwards;
        // The offset of the engine glow.
        private Vector2 _engineOffset;
        // Position of the ship.
        private Vector2 _position;
        // The laughable AI.
        private DummyAi _ai;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the speed of the ship.
        /// </summary>
        public float Speed
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the AI for this ship.
        /// </summary>
        public DummyAi Ai
        {
            get => _ai;
            set
            {
                _ai = value;
                if (_ai is not null)
                {
                    _ai.Ship = this;
                }
                UpdateEntityPositions();
            }
        }

        /// <summary>
        /// Property to set or return the angle of rotation, in degrees, for the ship.
        /// </summary>
        public float Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                UpdateEntityPositions();
            }
        }

        /// <summary>
        /// Property to set or return the position of the ship, in space.
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateEntityPositions();
            }
        }

        /// <summary>
        /// Property to set or return the layer used by the ship.
        /// </summary>
        /// <remarks>
        /// The object that has the controller will be responsible for moving the world around it.
        /// 
        /// Ideally we'd want to attach/detach the controller from the previous object prior to attaching here.  But for this example, and the sake of simplicty, we'll 
        /// just attach it as-is.
        /// </remarks>
        public LayerCamera LayerController
        {
            get => _layerController;
            set
            {
                if (_layerController is not null)
                {
                    _layerController.TrackingLayer = null;
                }

                _layerController = value;

                if (_layerController is not null)
                {
                    _layerController.TrackingLayer = _layer;
                }

                UpdateEntityPositions();
            }
        }
        #endregion

        #region Methods.		
        /// <summary>
        /// Function to update the entity positions to match the position of the ship.
        /// </summary>
        private void UpdateEntityPositions()
        {
            if (_layerController is not null)
            {
                _layerController.SetPosition(-_position);
            }

            if ((_ship is null) || (_engine is null))
            {
                return;
            }

            _ship.Rotation = _angle;
            _engine.Rotation = Speed >= 0 ? _angle : _angle + 180;
            _engine.LocalPosition = _ship.LocalPosition = _layerController is null ? _position : Vector2.Zero;
        }

        /// <summary>
        /// Function to rotate the ship in the specified direction.
        /// </summary>
        /// <param name="direction">The direction in which to rotate.</param>
        private void Rotate(int direction) => _angle += (direction * 45.0f) * GorgonTiming.Delta;

        /// <summary>
        /// Function to update the movement of the ship when stopping (i.e. the user hit backspace).
        /// </summary>
        private void UpdateStop()
        {
            // If we are slowing down (we hit backspace), then automatically decrement the speed (or increment if we're moving in reverse).
            if (!_isSlowing)
            {
                return;
            }

            if (_backwards)
            {
                Accelerate();
            }
            else
            {
                Decelerate();
            }

            // Ensure our speed locks at zero. 
            if (((_backwards) && (Speed >= 0))
                || (!_backwards) && (Speed <= 0))
            {
                _isSlowing = false;
                Speed = 0;
            }

            _backwards = Speed < 0;
        }

        /// <summary>
        /// Function to update the ship state per frame.
        /// </summary>
        public void Update()
        {
            // If we've an AI attached, then update it now so it can take control as needed.
            _ai?.Update();

            // Depending on our speed, fade the engine glow in/out.
            _engine.Color = new GorgonColor(_engine.Color, Speed.Abs() / (Speed >= 0 ? 1.0f : 1f));

            // If we're moving in reverse, then we can offset the engine glow to give the appearance of the 
            // engine burning from the other side of the ship.
            if (Speed < 0)
            {
                _engine.Anchor = new Vector2(_engineOffset.X, 0.73f);
            }
            else
            {
                _engine.Anchor = _engineOffset;
            }

            // If we have movement, then activate the animation for the engine, otherwise turn off the animation.
            if (!Speed.EqualsEpsilon(0))
            {
                if (_engineAnimationController.State != AnimationState.Playing)
                {
                    _engineAnimationController.Play(_engine.Sprite, _engine.Animation);
                }
            }
            else
            {
                _engineAnimationController.Stop();
            }

            UpdateStop();

            // Convert our angle and speed to a vector so we can get movement along it.
            float rads = _angle.ToRadians();
            Vector2 dirMag = new Vector2(-rads.Sin(), rads.Cos()) * Speed;
            Position -= dirMag * GorgonTiming.Delta;

            // Ensure our animation moves to the next frame.
            _engineAnimationController.Update();
        }

        /// <summary>
        /// Function to accelerate the ship.
        /// </summary>
        public void Accelerate()
        {
            Speed += 0.25f * GorgonTiming.Delta;
            _backwards = Speed < 0;
            Speed = Speed.Max(-0.8f).Min(1.0f);
        }

        /// <summary>
        /// Function to decelerate the ship.
        /// </summary>
        public void Decelerate()
        {
            Speed -= 0.25f * GorgonTiming.Delta;
            _backwards = Speed < 0;
            Speed = Speed.Max(-0.8f).Min(1.0f);
        }

        /// <summary>
        /// Function to perform a hard stop.
        /// </summary>
        /// <remarks>
        /// The AI object uses this to ensure the ship stays stationary after it has decelerated to a certain point (we don't want it going in reverse).
        /// </remarks>
        public void HardStop() => Speed = 0;

        /// <summary>
        /// Function to handle user input.
        /// </summary>
        /// <param name="keys">The keyboard key state.</param>
        /// <remarks>
        /// We use this intercept input from the keyboard. This allows the user to control the ship.  However, if we don't have the layer camera controller attached, 
        /// or we have an AI attached, then this method will have no effect.
        /// </remarks>
        public void UserInput(GorgonKeyStateCollection keys)
        {
            if ((_layerController is null) || (_ai is not null))
            {
                return;
            }

            if (keys[Keys.Back] == KeyState.Down)
            {
                _isSlowing = true;
            }

            if (keys[Keys.Left] == KeyState.Down)
            {
                Rotate(-1);
            }

            if (keys[Keys.Right] == KeyState.Down)
            {
                Rotate(1);
            }

            if (keys[Keys.Up] == KeyState.Down)
            {
                _isSlowing = false;
                Accelerate();
            }

            if (keys[Keys.Down] == KeyState.Down)
            {
                _isSlowing = false;
                Decelerate();
            }
        }

        /// <summary>
        /// Function to load the resources required for this ship.
        /// </summary>
        public void LoadResources()
        {
            // We'll need our sprite entities for this object so we can update their position and rotation.
            _ship = _layer.GetSpriteByName("Fighter");
            _engine = _layer.GetSpriteByName("EngineGlow");

            // Since we have no acceleration, hide the engine glow.
            _engine.Color = new GorgonColor(_engine.Color, 0);

            // Remember the original engine offset so we can revert to it as needed.
            _engineOffset = _engine.Anchor;

            UpdateEntityPositions();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Ship"/> class.</summary>
        /// <param name="layer">The layer.</param>
        public Ship(SpritesLayer layer)
        {
            _layer = layer;
            _engineAnimationController = new GorgonSpriteAnimationController();
        }
        #endregion
    }
}
