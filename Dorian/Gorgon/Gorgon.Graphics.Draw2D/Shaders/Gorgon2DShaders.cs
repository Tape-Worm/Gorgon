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
		private Gorgon2D _gorgon2D = null;								// 2D interface that owns this object.
		private Gorgon2DVertexShader _vertexShader = null;				// Current vertex shader.
		private Gorgon2DPixelShader _pixelShader = null;				// Current pixel shader.
		private GorgonConstantBuffer _viewProjection = null;			// Constant buffer for handling the view/projection matrix upload to the video device.
		private GorgonConstantBuffer _renderableStates = null;			// States for renderable objects.
		private GorgonMinMaxF? _alphaTestValue = null;					// Alpha test value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default verte shader.
		/// </summary>
		internal GorgonDefaultVertexShader DefaultVertexShader
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the default diffuse pixel shader.
		/// </summary>
		internal GorgonDefaultPixelShaderDiffuse DefaultPixelShaderDiffuse
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default textured pixel shader.
		/// </summary>
		internal GorgonDefaultPixelShaderTextured DefaultPixelShaderTextured
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default diffuse pixel shader with alpha testing.
		/// </summary>
		internal GorgonDefaultPixelShaderDiffuseAlphaTest DefaultPixelShaderDiffuseAlphaTest
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default textured pixel shader with alpha testing.
		/// </summary>
		internal GorgonDefaultPixelShaderTexturedAlphaTest DefaultPixelShaderTexturedAlphaTest
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the alpha test value to send to the shader.
		/// </summary>
		public GorgonMinMaxF? AlphaTestValue
		{
			get
			{
				return _alphaTestValue;
			}
			set
			{
				if (_alphaTestValue != value)
				{
					if (value.HasValue)
						_alphaTestValue = value.Value;
					else
						_alphaTestValue = GorgonMinMaxF.Empty;

					using (GorgonDataStream stream = _renderableStates.Lock(BufferLockFlags.Discard | BufferLockFlags.Write))
					{
						stream.Write(value.HasValue);

						// Pad out the boolean as it's only 1 byte in .NET.
						stream.Write<byte>(0);
						stream.Write<byte>(0);
						stream.Write<byte>(0);

						if (_alphaTestValue.HasValue)
						{
							stream.Write(_alphaTestValue.Value.Minimum);
							stream.Write(_alphaTestValue.Value.Maximum);
						}
						else
						{
							stream.Write(0.0f);
							stream.Write(0.0f);
						}
						_renderableStates.Unlock();
					}
				}
			}
		}
		
		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public Gorgon2DVertexShader VertexShader
		{
			get
			{
				return _vertexShader;
			}
			set
			{
				if ((_vertexShader != value) || ((value == null) && (_vertexShader != DefaultVertexShader)))
				{
					if (value == null)
						_vertexShader = DefaultVertexShader;
					else
						_vertexShader = value;

					if (value != null)
						_gorgon2D.Graphics.Shaders.VertexShader.Current = _vertexShader.Shader;
					else
						_gorgon2D.Graphics.Shaders.VertexShader.Current = DefaultVertexShader.Shader;

					_gorgon2D.Graphics.Shaders.VertexShader.ConstantBuffers[0] = _viewProjection;
				}
			}
		}

		/// <summary>
		/// Property to set or return the current pixel shader.
		/// </summary>
		public Gorgon2DPixelShader PixelShader
		{
			get
			{
				return _pixelShader;
			}
			set
			{
				if ((_pixelShader != value) || ((value == null) && (_pixelShader != DefaultPixelShaderTextured) && (_pixelShader != DefaultPixelShaderDiffuse) && (_pixelShader != DefaultPixelShaderDiffuseAlphaTest) && (_pixelShader != DefaultPixelShaderTexturedAlphaTest)))
				{
					if (value == null)
					{
						// If we have a texture in the first slot, then set the proper shader.
						if (_gorgon2D.Graphics.Shaders.PixelShader.Textures[0] == null)
							_pixelShader = DefaultPixelShaderDiffuse;
						else
							_pixelShader = DefaultPixelShaderTextured;
					}
					else
						_pixelShader = value;
					
					_gorgon2D.Graphics.Shaders.PixelShader.Current = _pixelShader.Shader;
					_gorgon2D.Graphics.Shaders.PixelShader.ConstantBuffers[0] = _renderableStates;
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
		/// Function to clean up the shader interface.
		/// </summary>
		internal void CleanUp()
		{
			if (_viewProjection != null)
				_viewProjection.Dispose();

			if (_renderableStates != null)
				_renderableStates.Dispose();

			if (DefaultVertexShader != null)
				DefaultVertexShader.Dispose();

			if (DefaultPixelShaderDiffuse != null)
				DefaultPixelShaderDiffuse.Dispose();

			if (DefaultPixelShaderTextured != null)
				DefaultPixelShaderTextured.Dispose();
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
			
			DefaultVertexShader = new GorgonDefaultVertexShader(gorgon2D);
			DefaultPixelShaderDiffuse = new GorgonDefaultPixelShaderDiffuse(gorgon2D);
			DefaultPixelShaderTextured = new GorgonDefaultPixelShaderTextured(gorgon2D);
			DefaultPixelShaderDiffuseAlphaTest = new GorgonDefaultPixelShaderDiffuseAlphaTest(gorgon2D);
			DefaultPixelShaderTexturedAlphaTest = new GorgonDefaultPixelShaderTexturedAlphaTest(gorgon2D);

			_viewProjection = gorgon2D.Graphics.Shaders.CreateConstantBuffer(DirectAccess.SizeOf<Matrix>(), true);
			_renderableStates = gorgon2D.Graphics.Shaders.CreateConstantBuffer(32, true);
		}
		#endregion
	}
}
