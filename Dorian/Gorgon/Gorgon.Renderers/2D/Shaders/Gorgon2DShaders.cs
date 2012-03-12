using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Native;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
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
		private GorgonConstantBuffer _alphaTest = null;					// Alpha testing data for the shader.
		private GorgonMinMaxF _alphaTestValue = GorgonMinMaxF.Empty;	// Alpha test value.
		private GorgonDataStream _alphaTestData = null;					// Data for constant buffer.
		private GorgonDataStream _projViewData = null;					// Data for constant buffer.
		private bool _alphaTestEnabled = false;							// Flag to indicate that alpha testing is enabled.
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
		/// Property to return the default diffuse pixel shader with alpha testing.
		/// </summary>
		internal GorgonDefaultPixelShaderDiffuse DefaultPixelShaderDiffuse
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default textured pixel shader with alpha testing.
		/// </summary>
		internal GorgonDefaultPixelShaderTextured DefaultPixelShaderTextured
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether alpha testing is enabled or not.
		/// </summary>
		public bool IsAlphaTestEnabled
		{
			get
			{
				return _alphaTestEnabled;
			}
			set
			{
				if (_alphaTestEnabled != value)
				{
					_alphaTestEnabled = value;
					if (value)
						_gorgon2D.Graphics.Shaders.PixelShader.ConstantBuffers[1] = _alphaTest;
					else
						_gorgon2D.Graphics.Shaders.PixelShader.ConstantBuffers[1] = null;
				}
			}
		}

		/// <summary>
		/// Property to set or return the alpha test value to send to the shader.
		/// </summary>
		public GorgonMinMaxF AlphaTestValue
		{
			get
			{
				return _alphaTestValue;
			}
			set
			{
				if (_alphaTestValue != value)
				{					
					_alphaTestValue = value;

					_alphaTestData.Write(IsAlphaTestEnabled);
					// Pad out the boolean as it's only 1 byte in .NET.
					_alphaTestData.Write<byte>(0);
					_alphaTestData.Write<byte>(0);
					_alphaTestData.Write<byte>(0);
					_alphaTestData.Write(value.Minimum);
					_alphaTestData.Write(value.Maximum);
					_alphaTestData.Position = 0;
					_alphaTest.Update(_alphaTestData);
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
				if ((_pixelShader != value) || ((value == null) && (_pixelShader != DefaultPixelShaderTextured) && (_pixelShader != DefaultPixelShaderDiffuse) && (_pixelShader != DefaultPixelShaderDiffuse) && (_pixelShader != DefaultPixelShaderTextured)))
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
					_gorgon2D.Graphics.Shaders.PixelShader.ConstantBuffers[0] = _viewProjection;
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
			Matrix viewProjection = Matrix.Multiply(_gorgon2D.CurrentCamera.View, _gorgon2D.CurrentCamera.Projection);

			_projViewData.Write(viewProjection);
			_projViewData.Position = 0;

			_viewProjection.Update(_projViewData);
		}

		/// <summary>
		/// Function to clean up the shader interface.
		/// </summary>
		internal void CleanUp()
		{
			if (_viewProjection != null)
				_viewProjection.Dispose();

			if (_alphaTest != null)
				_alphaTest.Dispose();

			if (_alphaTestData != null)
				_alphaTestData.Dispose();

			if (_projViewData != null)
				_projViewData.Dispose();

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

			_alphaTestData = new GorgonDataStream(32);
			_projViewData = new GorgonDataStream(DirectAccess.SizeOf<Matrix>());

			_viewProjection = gorgon2D.Graphics.Shaders.CreateConstantBuffer((int)_projViewData.Length, false);
			
			_alphaTestData.Write<int>(-1);
			_alphaTestData.Write(0.0f);
			_alphaTestData.Write(0.0f);
			_alphaTestData.Position = 0;

			_alphaTest = gorgon2D.Graphics.Shaders.CreateConstantBuffer((int)_alphaTestData.Length, false, _alphaTestData);
		}
		#endregion
	}
}
