#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 24, 2008 12:50:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics.Utilities
{
	/// <summary>
	/// Object representing a particle system emitter.
	/// </summary>
	public class ParticleEmitter
		: NamedObject
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
			public Vector2D Position;
			/// <summary>
			/// Velocity of the particle.
			/// </summary>
			public Vector2D Velocity;
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
			/// Red color of the particle.
			/// </summary>
			public float Red;
			/// <summary>
			/// Green color of the particle.
			/// </summary>
			public float Green;
			/// <summary>
			/// Blue color of the particle.
			/// </summary>
			public float Blue;
			/// <summary>
			/// Alpha color of the particle.
			/// </summary>
			public float Alpha;
			/// <summary>
			/// Red color of the particle.
			/// </summary>
			public float RedDelta;
			/// <summary>
			/// Green color of the particle.
			/// </summary>
			public float GreenDelta;
			/// <summary>
			/// Blue color of the particle.
			/// </summary>
			public float BlueDelta;
			/// <summary>
			/// Alpha color of the particle.
			/// </summary>
			public float AlphaDelta;
			/// <summary>
			/// Age of the particle.
			/// </summary>
			public float Age;
			/// <summary>
			/// Terminal age of the particle.
			/// </summary>
			public float TerminalAge;
			/// <summary>
			/// Flag to indicate that this particle is alive.
			/// </summary>
			public bool Alive;
			#endregion
		}
		#endregion

		#region Variables.
		private Random _rnd = new Random();					// Random number generator.
		private Particle[] _particles = null;				// List of particles.
		private Vector2D _previousPosition;					// Previous position.
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
		/// Property to set or return the life time of the emitter.
		/// </summary>
		public float EmitterLifetime
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the maximum particle count.
		/// </summary>
		public int MaximumParticleCount
		{
			get
			{
				return _particles.Length;
			}
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("The number of particles cannot be less than 1.");

				_particles = new Particle[value];

				for (int i = 0; i < value; i++)
					_particles[i] = new Particle();
			}
		}

		/// <summary>
		/// Property to return the age of the particles for this emitter.
		/// </summary>
		public float Age
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to set or return the transpose for the emitter.
		/// </summary>
		public Vector2D Transpose
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the emitter.
		/// </summary>
		public Vector2D Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the sprite to use for the particle.
		/// </summary>
		public Sprite ParticleSprite
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the direction of the emitted particles (in degrees).
		/// </summary>
		public float Direction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the spread of the emitted particles.
		/// </summary>
		public float Spread
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the particles are relative or not.
		/// </summary>
		public bool Relative
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the speed of the particles.
		/// </summary>
		public Range<float> ParticleSpeedRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the strength of the gravity.
		/// </summary>
		public Range<float> ParticleGravityRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the tangential acceleration of the particles.
		/// </summary>
		public Range<float> TangentialAccelerationRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the radial acceleration of the particles.
		/// </summary>
		/// <remarks>This will cause the particles to move faster as they get further from the emitter.</remarks>
		public Range<float> RadialAccelerationRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property set or return the range of colors.
		/// </summary>
		public Range<Drawing.Color> ColorRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the lifetime of the particles.
		/// </summary>
		public Range<float> ParticleLifetimeRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property set or return the range of sizes for the particles.
		/// </summary>
		public Range<float> ParticleSizeRange
		{
			get;
			set;
		}

		/// <summary>
		/// Property set or return the range of rotation for the particles (in degrees).
		/// </summary>
		public Range<float> ParticleRotationRange
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
		/// Function to return a random floating point value between the range specified.
		/// </summary>
		/// <param name="randomRange">Range for the floating point value.</param>
		/// <returns>Random number within the range.</returns>
		private float RandomFloat(Range<float> randomRange)
		{
			return ((float)_rnd.NextDouble() * (randomRange.End - randomRange.Start)) + randomRange.Start;
		}

		/// <summary>
		/// Function to initialize the particle system.
		/// </summary>
		/// <param name="frameDelta">Frame delta time.</param>
		private void Initialize(float frameDelta)
		{
			var deadParticles = from particle in _particles
								where !particle.Alive
								select particle;

			foreach (Particle particle in deadParticles) 
			{
				particle.Age = 0.0f;
				particle.TerminalAge = RandomFloat(ParticleLifetimeRange);
				if (particle.TerminalAge > 0.0f)
				{
					particle.Position = Vector2D.Add(_previousPosition, Vector2D.Multiply(Vector2D.Subtract(Position, _previousPosition), RandomFloat(new Range<float>(0, 1.0f))));
					particle.Position.X += RandomFloat(new Range<float>(-2.0f, 2.0f));
					particle.Position.Y += RandomFloat(new Range<float>(-2.0f, 2.0f));

					float angle = MathUtility.Radians(Direction) - (MathUtility.PI * 2.0f) + RandomFloat(new Range<float>(0, MathUtility.Radians(Spread))) - MathUtility.Radians(Spread) / 2.0f;
					if (Relative)
						angle += Vector2D.Subtract(_previousPosition, Position).Angle();
					particle.Velocity = new Vector2D(MathUtility.Cos(angle), MathUtility.Sin(angle));
					particle.Velocity = Vector2D.Multiply(particle.Velocity, RandomFloat(ParticleSpeedRange));

					particle.Gravity = RandomFloat(ParticleGravityRange);
					particle.RadialAcceleration = RandomFloat(RadialAccelerationRange);
					particle.TangentialAcceleration = RandomFloat(TangentialAccelerationRange);

					particle.Size = RandomFloat(new Range<float>(ParticleSizeRange.Start, ParticleSizeRange.Start + (ParticleSizeRange.End - ParticleSizeRange.Start)));
					particle.Spin = RandomFloat(new Range<float>(ParticleRotationRange.Start, ParticleRotationRange.Start + (ParticleRotationRange.End - ParticleRotationRange.Start)));
					particle.Red = RandomFloat(new Range<float>(ColorRange.Start.R, ColorRange.Start.R + (ColorRange.End.R - ColorRange.Start.R)));
					particle.Green = RandomFloat(new Range<float>(ColorRange.Start.G, ColorRange.Start.G + (ColorRange.End.G - ColorRange.Start.G)));
					particle.Blue = RandomFloat(new Range<float>(ColorRange.Start.B, ColorRange.Start.B + (ColorRange.End.B - ColorRange.Start.B)));
					particle.Alpha = RandomFloat(new Range<float>(ColorRange.Start.A, ColorRange.Start.A + (ColorRange.End.A - ColorRange.Start.A)));
					if (particle.Red > 255.0f)
						particle.Red = 255;
					if (particle.Green > 255.0f)
						particle.Green = 255;
					if (particle.Blue > 255.0f)
						particle.Blue = 255;
					if (particle.Alpha > 255.0f)
						particle.Alpha = 255;
					if (particle.Red < 0)
						particle.Red = 0;
					if (particle.Green < 0)
						particle.Green = 0;
					if (particle.Blue < 0)
						particle.Blue = 0;
					if (particle.Alpha < 0)
						particle.Alpha = 0;

					particle.SizeDelta = (ParticleSizeRange.End - particle.Size) / particle.TerminalAge;
					particle.SpinDelta = (ParticleRotationRange.End - particle.Spin) / particle.TerminalAge;
					particle.RedDelta = (ColorRange.End.R - particle.Red) / particle.TerminalAge;
					particle.GreenDelta = (ColorRange.End.G - particle.Green) / particle.TerminalAge;
					particle.BlueDelta = (ColorRange.End.B - particle.Blue) / particle.TerminalAge;
					particle.AlphaDelta = (ColorRange.End.A - particle.Alpha) / particle.TerminalAge;
					particle.Alive = true;
				}
			}
		}

		/// <summary>
		/// Function to reset the particle emitter.
		/// </summary>
		public void Reset()
		{
			Age = 0.0f;
			foreach (Particle particle in _particles)
				particle.Alive = false;
		}

		/// <summary>
		/// Function to move the particles to another location.
		/// </summary>
		/// <param name="x">Horizontal Position.</param>
		/// <param name="y">Vertical position.</param>
		public void Move(float x, float y)
		{
			Vector2D delta = Vector2D.Zero;			// Delta

			var aliveParticles = from particle in _particles
								 where particle.Alive
								 select particle;

			delta = Vector2D.Subtract(new Vector2D(x, y), Position);

			foreach (Particle particle in aliveParticles)
				particle.Position = Vector2D.Add(particle.Position, delta);

			_previousPosition = Vector2D.Add(_previousPosition, delta);
			Position = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to update the particles.
		/// </summary>
		/// <param name="frameDelta">Frame delta time.</param>
		public void Update(float frameDelta)
		{
			bool skipInit = false;			// Flag to skip initialization.

			var aliveParticles = from particle in _particles
								where particle.Alive
								select particle;

			if (Paused)
				return;

			if (!ContinuousParticles)
			{
				Age += frameDelta;
				if (Age >= EmitterLifetime)
					skipInit = true;				
			}
			
			foreach(Particle particle in aliveParticles)
			{
				if (particle.Age >= 0.0f)
					particle.Age += frameDelta;
				if ((particle.Age >= particle.TerminalAge) || (particle.Age < 0))
				{
					particle.Alive = false;
					continue;
				}

				Vector2D acceleration = Vector2D.Subtract(particle.Position, Position);
				acceleration.Normalize();
				acceleration = Vector2D.Multiply(acceleration, EmitterScale);
				Vector2D prevAcceleration = acceleration;
				acceleration = Vector2D.Multiply(acceleration, particle.RadialAcceleration);

				float angle = prevAcceleration.X;
				prevAcceleration.X = -prevAcceleration.Y;
				prevAcceleration.Y = angle;
				prevAcceleration = Vector2D.Multiply(prevAcceleration, particle.TangentialAcceleration);

				particle.Velocity = Vector2D.Add(particle.Velocity, Vector2D.Multiply(Vector2D.Add(acceleration, prevAcceleration), frameDelta));
				particle.Velocity.Y += particle.Gravity * frameDelta;

				particle.Position = Vector2D.Add(particle.Position, Vector2D.Multiply(particle.Velocity, frameDelta));

				particle.Spin += particle.SpinDelta * frameDelta;
				particle.Size += particle.SizeDelta * frameDelta;

				particle.Red += particle.RedDelta * frameDelta;
				particle.Green += particle.GreenDelta * frameDelta;
				particle.Blue += particle.BlueDelta * frameDelta;
				particle.Alpha += particle.AlphaDelta * frameDelta;

				if (particle.Red > 255.0f)
					particle.Red = 255;
				if (particle.Green > 255.0f)
					particle.Green = 255;
				if (particle.Blue > 255.0f)
					particle.Blue = 255;
				if (particle.Alpha > 255.0f)
					particle.Alpha = 255;
				if (particle.Red < 0)
					particle.Red = 0;
				if (particle.Green < 0)
					particle.Green = 0;
				if (particle.Blue < 0)
					particle.Blue = 0;
				if (particle.Alpha < 0)
					particle.Alpha = 0;
			}

			if (!skipInit)
				Initialize(frameDelta);

			_previousPosition = Position;
		}

		/// <summary>
		/// Function to draw the particles.
		/// </summary>
		public void Draw()
		{
			var aliveParticles = from particle in _particles
								where particle.Alive
								select particle;

			foreach(Particle particle in aliveParticles)
			{
				ParticleSprite.Color = Drawing.Color.FromArgb((int)particle.Alpha, (int)particle.Red, (int)particle.Green, (int)particle.Blue);
				ParticleSprite.Position = Vector2D.Add(particle.Position, Transpose);
				ParticleSprite.Rotation = MathUtility.Degrees(particle.Spin * particle.Age);
				ParticleSprite.UniformScale = particle.Size * EmitterScale;
				ParticleSprite.Draw();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ParticleEmitter"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <param name="particleSprite">Sprite to use for the particle.</param>
		/// <param name="position">Position of the emitter.</param>
		/// <remarks>
		/// Since the Name property is read-only, the name for an object must be passed to this constructor.
		/// <para>Typically it will be difficult to rename an object that resides in a collection with its name as the key, thus making the property writable would be counter-productive.</para>
		/// 	<para>However, objects that descend from this object may choose to implement their own renaming scheme if they so choose.</para>
		/// 	<para>Ensure that the name parameter has a non-null string or is not a zero-length string.  Failing to do so will raise a <see cref="System.ArgumentNullException">ArgumentNullException</see>.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public ParticleEmitter(string name, Sprite particleSprite, Vector2D position)
			: base(name)
		{
			if (particleSprite == null)
				throw new ArgumentNullException("particleSprite");

			Paused = false;
			EmitterScale = 1.0f;
			_previousPosition = Position = position;
			ContinuousParticles = true;
			ParticlesPerSecond = 1000;
			ParticleSprite = particleSprite;
			MaximumParticleCount = 500;
			Initialize(1.0f);
		}
		#endregion
	}
}
