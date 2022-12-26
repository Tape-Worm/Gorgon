#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: August 14, 2020 10:12:30 AM
// 
#endregion

using System;
using System.Linq;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// Object representing a particle system emitter.
/// </summary>
/// <remarks>
/// <para>
/// The emitter is responsible for creating and managing the lifetime of the particles.
/// 
/// This code was adapted from the HGE particle system code available at https://github.com/kvakvs/hge/
/// </para>
/// </remarks>
public class ParticleEmitter
{
    #region Classes.
    /// <summary>
    /// Object representing a particle.
    /// </summary>
    private class Particle
    {
        #region Variables.
        /// <summary>
        /// Position of the particle.
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// Velocity of the particle.
        /// </summary>
        public Vector2 Velocity;
        /// <summary>
        /// Gravity for the particle.
        /// </summary>
        public float Gravity;
        /// <summary>
        /// Radial acceleration for the particle.
        /// </summary>
        public float RadialAcceleration;
        /// <summary>
        /// Tangential acceleration for the particle.
        /// </summary>
        public float TangentialAcceleration;
        /// <summary>
        /// Rotation of the particle.
        /// </summary>
        public float Spin;
        /// <summary>
        /// Delta for the rotation.
        /// </summary>
        public float SpinDelta;
        /// <summary>
        /// Size of the particle.
        /// </summary>
        public float Size;
        /// <summary>
        /// Delta for the size.
        /// </summary>
        public float SizeDelta;
        /// <summary>
        /// Color for the particle sprite.
        /// </summary>
        public GorgonColor Color;
        /// <summary>
        /// Delta change for the color.
        /// </summary>
        public Vector4 ColorDelta;
        /// <summary>
        /// Age of the particle.
        /// </summary>
        public float Age;
        /// <summary>
        /// Terminal age of the particle.
        /// </summary>
        public float TerminalAge;
        #endregion

        #region Methods
        /// <summary>
        /// Function to copy the contents of this particle to another particle.
        /// </summary>
        /// <param name="particle">The particle to copy into.</param>
        public void CopyTo(Particle particle)
        {
            particle.Position = Position;
            particle.Velocity = Velocity;
            particle.Gravity = Gravity;
            particle.RadialAcceleration = RadialAcceleration;
            particle.TangentialAcceleration = TangentialAcceleration;
            particle.Spin = Spin;
            particle.SpinDelta = SpinDelta;
            particle.Size = Size;
            particle.SizeDelta = SizeDelta;
            particle.Color = Color;
            particle.ColorDelta = ColorDelta;
            particle.Age = Age;
            particle.TerminalAge = TerminalAge;
        }
        #endregion
    }
    #endregion

    #region Variables.
    // List of particles.
    private Particle[] _particles = null;
    // Previous position.
    private Vector2 _previousPosition;
    // The renderer used to draw the particle(s).
    private readonly Gorgon2D _renderer;
    // The position of the emitter.
    private Vector2 _position;
    // The number of particles that are still active.
    private int _aliveCount;
    // The fractional remainder for the particle allocation calculation.
    private float _remainder;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the scale for the emitter.
    /// </summary>
    public float EmitterScale
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether we should have a continuous particle stream.
    /// </summary>
    /// <remarks>
    /// If this value is set to true, then the emitter liftime, and age properties have no meaning.
    /// </remarks>
    public bool ContinuousParticles
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of particles per second.
    /// </summary>
    public int ParticlesPerSecond
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the life time, in seconds, of the emitter.
    /// </summary>
    /// <remarks>
    /// This is only used if the ContinousParticles property is set to true.
    /// </remarks>
    public float EmitterLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the maximum number of particles that can be emitted. 
    /// </summary>
    /// <remarks>
    /// Larger values will have a negative impact on performance.
    /// </remarks>
    public int MaximumParticleCount
    {
        get => _particles.Length;
        set
        {
            value = value.Max(1);

            _particles = new Particle[value];

            for (int i = 0; i < value; i++)
            {
                _particles[i] = new Particle();
            }
        }
    }

    /// <summary>
    /// Property to return the age, in seconds, for the emitter.
    /// </summary>
    /// <remarks>
    /// This value does not matter if the ContinuousParticles property is set to true.
    /// </remarks>
    public float Age
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the position of the emitter.
    /// </summary>
    public ref Vector2 Position => ref _position;

    /// <summary>
    /// Property to set or return the sprite to use for the particle.
    /// </summary>
    public GorgonSprite ParticleSprite
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the direction, in degrees, of the emitted particles.
    /// </summary>
    public float Direction
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the spread, in degrees, of the emitted particles.
    /// </summary>
    public float Spread
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the particles are emitted relative to the emitter or not.
    /// </summary>
    public bool Relative
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial speed of new particles.
    /// </summary>
    public (float Minimum, float Maximum) ParticleSpeedRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial strength of the gravity for new particles.
    /// </summary>
    /// <remarks>
    /// A negative value will make the particle move in the opposite directory. 
    /// </remarks>
    public (float Minimum, float Maximum) ParticleGravityRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial tangential acceleration of new particles.
    /// </summary>
    /// <remarks>
    /// This will cause the particles to rotate (orbit) the emitter position.
    /// </remarks>
    public (float Minimum, float Maximum) TangentialAccelerationRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial radial acceleration of new particles.
    /// </summary>
    /// <remarks>
    /// This will cause the particles to move faster as they get further from the emitter.
    /// </remarks>
    public (float Minimum, float Maximum) RadialAccelerationRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property set or return the initial range of colors for new particles..
    /// </summary>
    public (GorgonColor Minimum, GorgonColor Maximum) ColorRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the initial lifetime of new particles, in seconds.
    /// </summary>
    public (float Minimum, float Maximum) ParticleLifetimeRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property set or return the initial range of sizes for new particles.
    /// </summary>
    public (float Minimum, float Maximum) ParticleSizeRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property set or return the initial range of rotation for new particles (in degrees).
    /// </summary>
    public (float Minimum, float Maximum) ParticleRotationRange
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the emitter is paused or not.
    /// </summary>
    public bool Paused
    {
        get;
        set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to create and initialize the particles.
    /// </summary>
    /// <param name="delta">The current frame delta time.</param>
    private void CreateParticles(float delta)
    {
        const float pi2 = (float)(System.Math.PI * 2.0);
        float updateCount = ParticlesPerSecond * delta + _remainder;
        int particleCount = (int)updateCount;
        _remainder = updateCount - particleCount;

        for (int i = 0; i < particleCount && _aliveCount < MaximumParticleCount; i++)
        {
            Particle particle = _particles[_aliveCount++];

            particle.Age = 0.0f;
            particle.TerminalAge = GorgonRandom.RandomSingle(ParticleLifetimeRange.Minimum, ParticleLifetimeRange.Maximum);

            if (particle.TerminalAge > 0.0f)
            {
                var posDelta = Vector2.Subtract(_position, _previousPosition);
                var posVar = Vector2.Multiply(posDelta, GorgonRandom.RandomSingle());
                particle.Position = Vector2.Add(_previousPosition, posVar);
                particle.Position.X += GorgonRandom.RandomSingle(-2, 2);
                particle.Position.Y += GorgonRandom.RandomSingle(-2, 2);
                
                float spreadRad = Spread.ToRadians();
                float angle = Direction.ToRadians() - pi2 + GorgonRandom.RandomSingle(0, spreadRad) - spreadRad * 0.5f;
                if (Relative)
                {
                    var diff = Vector2.Subtract(_previousPosition, _position);                        
                    angle += diff.Y.ATan(diff.X) + pi2;
                }

                particle.Velocity = new Vector2(angle.Cos(), angle.Sin());
                particle.Velocity = Vector2.Multiply(particle.Velocity, GorgonRandom.RandomSingle(ParticleSpeedRange.Minimum, ParticleSpeedRange.Maximum));

                particle.Gravity = GorgonRandom.RandomSingle(ParticleGravityRange.Minimum, ParticleGravityRange.Maximum);
                particle.RadialAcceleration = GorgonRandom.RandomSingle(RadialAccelerationRange.Minimum, RadialAccelerationRange.Maximum);
                particle.TangentialAcceleration = GorgonRandom.RandomSingle(TangentialAccelerationRange.Minimum, TangentialAccelerationRange.Maximum);

                particle.Size = GorgonRandom.RandomSingle(ParticleSizeRange.Minimum, ParticleSizeRange.Maximum);
                particle.Spin = GorgonRandom.RandomSingle(ParticleRotationRange.Minimum, ParticleRotationRange.Maximum);
                particle.Color = GorgonColor.Clamp(new GorgonColor(GorgonRandom.RandomSingle(ColorRange.Minimum.Red, ColorRange.Maximum.Red),
                                                 GorgonRandom.RandomSingle(ColorRange.Minimum.Green, ColorRange.Maximum.Green),
                                                 GorgonRandom.RandomSingle(ColorRange.Minimum.Blue, ColorRange.Maximum.Blue),
                                                 GorgonRandom.RandomSingle(ColorRange.Minimum.Alpha, ColorRange.Maximum.Alpha)));

                particle.SizeDelta = (ParticleSizeRange.Maximum - particle.Size) / particle.TerminalAge;
                particle.SpinDelta = (ParticleRotationRange.Maximum - particle.Spin) / particle.TerminalAge;
                particle.ColorDelta = new Vector4((ColorRange.Maximum.Red - particle.Color.Red) / particle.TerminalAge,
                                                     (ColorRange.Maximum.Green - particle.Color.Green) / particle.TerminalAge,
                                                     (ColorRange.Maximum.Blue - particle.Color.Blue) / particle.TerminalAge,
                                                     (ColorRange.Maximum.Alpha - particle.Color.Alpha) / particle.TerminalAge);
            }
        }
    }

    /// <summary>
    /// Function to reset the particle emitter.
    /// </summary>
    public void Reset()
    {
        _aliveCount = 0;
        Age = 0.0f;
    }

    /// <summary>
    /// Function to move the particles to another location.
    /// </summary>
    /// <param name="x">Horizontal Position.</param>
    /// <param name="y">Vertical position.</param>
    public void Move(float x, float y)
    {
        var offset = new Vector2(x, y);

        var delta = Vector2.Subtract(offset, _position);

        for (int i = 0; i < _aliveCount; i++)
        {
            Particle particle = _particles[i];

            particle.Position = Vector2.Add(particle.Position, delta);
        }

        _previousPosition = Vector2.Add(_previousPosition, delta);
        Position = offset;
    }

    /// <summary>
    /// Function to update the particles.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called ever frame so that the particles will actually move and expire.
    /// </para>
    /// </remarks>
    public void Update()
    {
        // Flag to skip initialization.
        float frameDelta = GorgonTiming.Delta;

        if ((Paused) || (ParticleSprite is null))
        {
            return;
        }

        if (!ContinuousParticles)
        {
            Age += frameDelta;
        }

        int i = 0;
        while (i < _aliveCount)
        {
            Particle particle = _particles[i];

            particle.Age += frameDelta;

            if (particle.Age >= particle.TerminalAge)
            {
                _particles[--_aliveCount].CopyTo(_particles[i]);
                continue;
            }

            var acceleration = Vector2.Normalize(Vector2.Subtract(particle.Position, _position));

            var crossAcceleration = new Vector2(-acceleration.Y, acceleration.X);

            var radialAcceleration = Vector2.Multiply(acceleration, particle.RadialAcceleration);
            var tangentAcceleration = Vector2.Multiply(crossAcceleration, particle.TangentialAcceleration);

            var totalAccel = Vector2.Add(radialAcceleration, tangentAcceleration);
            var finalAccel = Vector2.Multiply(totalAccel, frameDelta);
            particle.Velocity = Vector2.Add(particle.Velocity, finalAccel);
            particle.Velocity.Y += particle.Gravity * frameDelta;

            var finalVel = Vector2.Multiply(particle.Velocity, frameDelta);
            particle.Position = Vector2.Add(particle.Position, finalVel);

            particle.Spin += particle.SpinDelta * frameDelta;
            particle.Size += particle.SizeDelta * frameDelta;

            particle.Color = GorgonColor.Clamp(new GorgonColor(particle.Color.Red + particle.ColorDelta.X * frameDelta,
                                             particle.Color.Green + particle.ColorDelta.Y * frameDelta,
                                             particle.Color.Blue + particle.ColorDelta.Z * frameDelta,
                                             particle.Color.Alpha + particle.ColorDelta.W * frameDelta));
            ++i;
        }

        // If the emitter isn't at the end of its lifetime, create new particles (if any particles need to be created).
        if (Age < EmitterLifetime)
        {
            CreateParticles(frameDelta);
        }

        _previousPosition = Position;
    }

    /// <summary>
    /// Function to draw the particles.
    /// </summary>
    public void Draw()
    {
        if (ParticleSprite is null)
        {
            return;
        }

        for (int i = 0; i < _aliveCount; i++)
        {
            Particle particle = _particles[i];

            ParticleSprite.Color = particle.Color;
            ParticleSprite.Position = particle.Position;
            ParticleSprite.Angle = (particle.Spin * particle.Age).ToDegrees();
            ParticleSprite.Scale = new Vector2(particle.Size * EmitterScale);
            _renderer.DrawSprite(ParticleSprite);
        }
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleEmitter"/> class.
    /// </summary>
    /// <param name="renderer">The 2D renderer used to render the particles.</param>
    /// <param name="particleSprite">Sprite to use for the particle.</param>
    /// <param name="position">The initial position of the emitter.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or <paramref name="particleSprite"/> parameter is <b>null</b>.</exception>
    public ParticleEmitter(Gorgon2D renderer, GorgonSprite particleSprite, Vector2 position)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        ParticleSprite = particleSprite ?? throw new ArgumentNullException(nameof(particleSprite));

        // Set up some default values.
        ParticlesPerSecond = 1000;
        MaximumParticleCount = 500;

        Relative = true;
        Paused = false;

        _previousPosition = Position = position;
        ContinuousParticles = true;

        EmitterScale = 1.0f;
        EmitterLifetime = 15.0f;

        ParticleGravityRange = (0, 0);
        ParticleLifetimeRange = (5.0f, 5.0f);
        ParticleSizeRange = (1.0f, 0.01f);
        ParticleSpeedRange = (300, 300);
        TangentialAccelerationRange = (0.0f, 0.0f);
        RadialAccelerationRange = (0.0f, 0.0f);
        ParticleRotationRange = (0.0f, 0.0f);
        Spread = 360.0f;
        Direction = 0.0f;            
        ColorRange = (GorgonColor.LightYellow, new GorgonColor(GorgonColor.OrangeRed, 0.0f));            

        // Force particle creation (assume 60 FPS).
        CreateParticles(1 / 60.0f);
    }
    #endregion
}
