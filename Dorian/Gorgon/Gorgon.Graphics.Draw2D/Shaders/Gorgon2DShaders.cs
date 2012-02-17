using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// An interface for shader functionality.
	/// </summary>
	public class Gorgon2DShaders
	{
		#region Variables.
		private Gorgon2D _gorgon2D = null;						// 2D interface that owns this object.
		private Gorgon2DShader _current = null;					// The current 2D shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the constant buffer for the view/projection buffer.
		/// </summary>
		internal GorgonConstantBuffer ViewProjection
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the default shader.
		/// </summary>
		internal GorgonDefaultShader DefaultShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		public Gorgon2DShader Current
		{
			get
			{
				if (_current == null)
					return DefaultShader;

				return _current;
			}
			set
			{
				if (_current != value)
				{
					if (_current != null)
					{
						_current = value;
						UpdateShaders();
					}
					else
						DefaultShader.SetDefault();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the default constants for the shader(s).
		/// </summary>
		private void UpdateShaders()
		{
			_gorgon2D.Graphics.Shaders.VertexShader.ConstantBuffers[0] = ViewProjection;
		}

		/// <summary>
		/// Function to gorgon's transformation matrix.
		/// </summary>
		internal void UpdateGorgonTransformation()
		{
			using (GorgonDataStream streamBuffer = ViewProjection.Lock(BufferLockFlags.Discard | BufferLockFlags.Write))
			{
				Matrix viewProjection = Matrix.Multiply(_gorgon2D.ViewMatrix.Value, _gorgon2D.ProjectionMatrix.Value);
				streamBuffer.Write(viewProjection);
				ViewProjection.Unlock();
			}
		}

		/// <summary>
		/// Function to clean up the shader interface.
		/// </summary>
		internal void CleanUp()
		{
			if (ViewProjection != null)
				ViewProjection.Dispose();

			if (DefaultShader != null)
				DefaultShader.Dispose();	
		}

		/// <summary>
		/// Function to create a shader object.
		/// </summary>
		/// <param name="name">Name of the shader object.</param>
		/// <returns>A new shader object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public Gorgon2DShader CreateShader(string name)
		{
			Gorgon2DShader result = null;

			result = new Gorgon2DShader(_gorgon2D, name);

			_gorgon2D.TrackedObjects.Add(result);
			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DShaders"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		internal Gorgon2DShaders(Gorgon2D gorgon2D)
		{
			_gorgon2D = gorgon2D;
			DefaultShader = new GorgonDefaultShader(gorgon2D);
			ViewProjection = gorgon2D.Graphics.Shaders.CreateConstantBuffer(DirectAccess.SizeOf<Matrix>(), true);
		}
		#endregion
	}
}
