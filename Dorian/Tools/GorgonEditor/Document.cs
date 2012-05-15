using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using KRBTabControl;
using SlimMath;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// A document for the editor.
	/// </summary>
	public class Document		
		: INamedObject, IDisposable
	{
		#region Events.
		/// <summary>
		/// Event fired when the properties of the document are updated.
		/// </summary>
		[Browsable(false)]
		public event EventHandler PropertyUpdated;
		#endregion

		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private GorgonTexture2D _logo = null;					// Logo.
		private RectangleF[] _blurStates = null;				// Images for blur states.
		private RectangleF _sourceState = RectangleF.Empty;		// Source image state.
		private RectangleF _destState = RectangleF.Empty;		// Destination image state.
		private string _name = string.Empty;					// Name of the document.
		private float _alphaDelta = 0.025f;						// Alpha delta value.
		private float _alpha = 0.0f;							// Alpha value.
		private GorgonFont _debugFont = null;					// Debug font.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type descriptor for this document.
		/// </summary>
		internal DocumentTypeDescriptor TypeDescriptor
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the renderer interface.
		/// </summary>
		[Browsable(false)]
		public Gorgon2D Renderer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the swap chain used for rendering in this document.
		/// </summary>
		protected GorgonSwapChain SwapChain
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the tab page that this document resides in.
		/// </summary>
		[Browsable(false)]
		public TabPageEx Tab
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the editor control for this document.
		/// </summary>
		[Browsable(false)]
		public Control Control
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the control that will receive rendering.
		/// </summary>
		[Browsable(false)]
		public Control RenderWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the name for the document.
		/// </summary>
		[Browsable(true), Category("Design"), Description("Provides a name for the object.")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					return;

				if (string.Compare(_name, value, false) != 0)
				{
					string previousName = _name;
					_name = value;

					// Update the collection.
					Program.Documents.Rename(previousName, value);

					Tab.Text = value;
					Tab.Name = "tab" + Name;

					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the object uses the property manager.
		/// </summary>
		[Browsable(false)]
		public bool HasProperties
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to dispatch an update notification.
		/// </summary>
		protected void DispatchUpdateNotification()
		{
			if (PropertyUpdated != null)
				PropertyUpdated(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>Number of vertical retraces to wait.</returns>
		protected virtual int Draw(GorgonFrameRate timing)
		{
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			float aspect = 0.0f;

			logoSize.Height = 124;

			if (Control.ClientSize.Width < logoSize.Width)
				logoBounds.Width = logoSize.Width * Control.ClientSize.Width / logoSize.Width;
			else
				logoBounds.Width = logoSize.Width;

			aspect = logoSize.Height / logoSize.Width;
			logoBounds.Height = logoBounds.Width * aspect;

			logoBounds.X = Control.ClientSize.Width / 2.0f - logoBounds.Width / 2.0f;
			logoBounds.Y = Control.ClientSize.Height / 2.0f - logoBounds.Height / 2.0f;

			Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;
			Renderer.Drawing.FilledRectangle(new RectangleF(2, logoBounds.Y, Control.ClientSize.Width - 4, logoBounds.Height), Color.FromArgb(160, 160, 160));

			Renderer.Drawing.BlendingMode = BlendingMode.PreMultiplied;
			Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f - _alpha), _logo, _sourceState);
			Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, _alpha), _logo, _destState);

			_alpha += _alphaDelta * timing.FrameDelta;

			if ((_alpha > 1.0f) && (_alphaDelta > 0))
			{
				if (_destState != _blurStates[0])
				{
					_sourceState = _blurStates[1];
					_destState = _blurStates[0];
					_alpha = 0.0f;
				}
				else
				{
					_destState = _blurStates[0];
					_sourceState = _blurStates[1];
					_alpha = 1.0f;
					_alphaDelta = -_alphaDelta;
				}
			}

			if ((_alpha < 0.0f) && (_alphaDelta < 0))
			{
				if (_destState != _blurStates[1])
				{
					_destState = _blurStates[1];
					_sourceState = _blurStates[2];
					_alpha = 1.0f;
				}
				else
				{
					_destState = _blurStates[1];
					_sourceState = _blurStates[2];
					_alpha = 0.0f;
					_alphaDelta = -_alphaDelta;
				}
			}

			Renderer.Drawing.DrawString(_debugFont, "FPS: " + timing.FPS.ToString("0.0##"), Vector2.Zero, Color.White);

			return 2;
		}

		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected virtual void LoadGraphics()
		{
			SizeF logoSize = SizeF.Empty;
			GorgonRenderTarget blur = null;	// Medium blur.

			_logo = Program.Graphics.Textures.FromGDIBitmap("Logo", Properties.Resources.Gorgon_2_Logo_Full, new GorgonTexture2DSettings()
				{
					Width = 0,
					Height = 372,
					FileFilter = ImageFilters.None,
					FileMipFilter = ImageFilters.None,
					Format = BufferFormat.Unknown,
					ArrayCount = 1,
					IsTextureCube = false,
					MipCount = 1,
					Usage = BufferUsage.Default
				});

			logoSize = new SizeF(_logo.Settings.Width, 124.0f);

			blur = Program.Graphics.Output.CreateRenderTarget("Blur.Medium", new GorgonRenderTargetSettings()
				{
					Width = (int)logoSize.Width,
					Height = (int)logoSize.Height,
					Format = _logo.Settings.Format				
				});

			try
			{
				// Blur the logo.
				blur.Clear(Control.BackColor);

				// Maximum blur.
				Renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(64, 64);
				Renderer.Effects.GaussianBlur.BlurAmount = 3.2f;
				Renderer.Effects.GaussianBlur.Render((int pass) =>
					{
						if (pass == 0)
							Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize), Color.White, _logo, new RectangleF(Vector2.Zero, logoSize));
						else
						{
							Renderer.Target = blur;
							Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, logoSize), Color.White, Renderer.Effects.GaussianBlur.BlurredTexture, new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize));
						}
					});
				_logo.CopySubResource(blur.Texture, new Rectangle(0, 0, blur.Settings.Width, blur.Settings.Height), new Vector2(0, 124));
				
				// Medium blur.
				Renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(128, 128);
				Renderer.Effects.GaussianBlur.BlurAmount = 2.7f;
				Renderer.Effects.GaussianBlur.Render((int pass) =>
				{
					if (pass == 0)
						Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize), Color.White, _logo, new RectangleF(Vector2.Zero, logoSize));
					else
					{
						Renderer.Target = blur;
						Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, logoSize), Color.White, Renderer.Effects.GaussianBlur.BlurredTexture, new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize));
					}
				});
				_logo.CopySubResource(blur.Texture, new Rectangle(0, 0, blur.Settings.Width, blur.Settings.Height), new Vector2(0, 248));

				_blurStates = new[] {
					new RectangleF(0, 0, _logo.Settings.Width, 124),		// No blur.
					new RectangleF(0, 248, _logo.Settings.Width, 124),		// Medium blur.
					new RectangleF(0, 124, _logo.Settings.Width, 124),		// Max blur.
					};
				
				_sourceState = _blurStates[2];
				_destState = _blurStates[1];

				Renderer.Target = null;				
			}
			finally
			{
				Renderer.Effects.GaussianBlur.FreeResources();

				if (blur != null)
					blur.Dispose();
			}

			_debugFont = Program.Graphics.Textures.CreateFont("My font", new GorgonFontSettings()
			{
				AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
				FontFamilyName = "Arial",
				FontStyle = FontStyle.Bold,
				FontHeightMode = FontHeightMode.Points,
				Size = 14.0f
			});
		}

		/// <summary>
		/// Function to remove graphics and other items for the document.
		/// </summary>
		protected virtual void UnloadGraphics()
		{
			if (_logo != null)
				_logo.Dispose();

			_logo = null;
		}

		/// <summary>
		/// Function to retrieve default values for properties with the DefaultValue attribute.
		/// </summary>
		public void SetDefaults()
		{
			foreach (var descriptor in TypeDescriptor)
			{
				if (descriptor.HasDefaultValue)
					descriptor.DefaultValue = descriptor.GetValue<object>();
			}
		}
		
		/// <summary>
		/// Function to call for rendering.
		/// </summary>
		/// <param name="timing">Timing data to pass to the method.</param>
		/// <returns>TRUE to continue running, FALSE to exit.</returns>
		public void RenderMethod(GorgonFrameRate timing)
		{
			if ((SwapChain == null) || (Control == null) || (Renderer == null))
				return;

			Renderer.Target = null;
			SwapChain.Clear(Control.BackColor);			
			Renderer.Render(Draw(timing));
		}

		/// <summary>
		/// Function to initialize the document.
		/// </summary>
		public void InitializeRendering()
		{
			if (Control == null)
				return;

			TerminateRendering();

			SwapChain = Program.Graphics.Output.CreateSwapChain("Document.SwapChain." + Name, new GorgonSwapChainSettings()
			{
				BufferCount = 2,
				DepthStencilFormat = BufferFormat.Unknown,
				Flags = SwapChainUsageFlags.RenderTarget,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				IsWindowed = true,
				Window = RenderWindow
			});

			Renderer = Program.Graphics.Create2DRenderer(SwapChain);

			LoadGraphics();
		}

		/// <summary>
		/// Functino to terminate the document.
		/// </summary>
		public void TerminateRendering()
		{
			UnloadGraphics();

			if (Renderer != null)
				Renderer.Dispose();

			if (SwapChain != null)
				SwapChain.Dispose();

			Renderer = null;
			SwapChain = null;
		}

		/// <summary>
		/// Function to update the rendering control.
		/// </summary>
		/// <param name="newControl">New control to use.</param>
		/// <param name="renderControl">Control to receive rendering.</param>
		protected void UpdateControl(Control newControl, Control renderControl)
		{
			bool needReinit = false;		// Flag to indicate that we need to reinitialize the renderer.

			if (SwapChain != null)
			{
				TerminateRendering();
				needReinit = true;
			}

			if (Control != null)
				Control.Dispose();
			Control = newControl;
			if (renderControl == null)
				RenderWindow = Control;
			else
				RenderWindow = renderControl;
			Tab.Controls.Add(Control);

			if (needReinit)
				InitializeRendering();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Document"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to keep open.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public Document(string name, bool allowClose)
		{
			GorgonDebug.AssertParamString(name, "name");

			_name = name;
			Tab = new TabPageEx(name);
			Tab.Name = "tab" + name;
			Control = new Panel();
			Control.Name = "panel" + name;
			Control.BackColor = Color.FromKnownColor(KnownColor.DimGray);
			Control.Dock = DockStyle.Fill;
			RenderWindow = Control;
			Tab.Controls.Add(Control);
			Tab.IsClosable = allowClose;

			TypeDescriptor = new DocumentTypeDescriptor(this);
			TypeDescriptor.Enumerate(GetType());
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					TerminateRendering();
					if (Control != null)
						Control.Dispose();
					if ((Tab != null) && (!Tab.IsClosable))
						Tab.Dispose();
				}

				RenderWindow = null;
				Control = null;
				Tab = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return _name;
			}
		}
		#endregion
	}
}
