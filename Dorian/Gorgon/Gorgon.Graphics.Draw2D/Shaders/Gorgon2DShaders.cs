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
		private GorgonDefaultShader _default = null;			// The default 2D shader.
		private GorgonConstantBuffer _viewProjection = null;	// The view/projection matrix.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		public Gorgon2DShader Current
		{
			get
			{
				if (_current == null)
					return _default;
				return _current;
			}
			set
			{
				if (_current != null)
				{
					_current = value;
					UpdateShaders();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to gorgon's transformation matrix.
		/// </summary>
		internal void UpdateGorgonTransformation()
		{
			using (GorgonDataStream streamBuffer = _viewProjection.Lock(BufferLockFlags.Discard | BufferLockFlags.Write))
			{
				Matrix viewProjection = Matrix.Multiply(_gorgon2D.ViewMatrix.Value, _gorgon2D.ProjectionMatrix.Value);
				streamBuffer.Write(viewProjection);
				_viewProjection.Unlock();
			}
		}

		/// <summary>
		/// Function to update the shaders.
		/// </summary>
		internal void UpdateShaders()
		{
			_gorgon2D.Graphics.Shaders.PixelShader.Current = Current.PixelShader;
			_gorgon2D.Graphics.Shaders.VertexShader.Current = Current.VertexShader;
			_gorgon2D.Graphics.Shaders.VertexShader.ConstantBuffers[0] = _viewProjection;
		}

		/// <summary>
		/// Function to clean up the shader interface.
		/// </summary>
		internal void CleanUp()
		{
			if (_viewProjection != null)
				_viewProjection.Dispose();

			if (_default != null)
				_default.Dispose();	
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
			_default = new GorgonDefaultShader(gorgon2D);
			_viewProjection = gorgon2D.Graphics.Shaders.CreateConstantBuffer(DirectAccess.SizeOf<Matrix>(), true);
		}
		#endregion
	}
}
