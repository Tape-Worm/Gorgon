using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
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
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;				// Flag to indicate that the object was disposed.
		private GorgonTexture2D _logo = null;		// Logo.
		private float _blurDelta = 1.0f;			// Blur delta.
		private float _blurAmount = 2.1f;			// Blur amount.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the renderer interface.
		/// </summary>
		protected Gorgon2D Renderer
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
		public TabPageEx Tab
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the editor control for this document.
		/// </summary>
		public Control Control
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		protected virtual void Draw(GorgonFrameRate timing)
		{
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			float aspect = 0.0f;
			float alpha = 0.0f;

			if (Control.ClientSize.Width < logoSize.Width)
				logoBounds.Width = logoSize.Width * Control.ClientSize.Width / logoSize.Width;
			else
				logoBounds.Width = logoSize.Width;

			aspect = logoSize.Height / logoSize.Width;
			logoBounds.Height = logoBounds.Width * aspect;

			logoBounds.X = Control.ClientSize.Width / 2.0f - logoBounds.Width / 2.0f;
			logoBounds.Y = Control.ClientSize.Height / 2.0f - logoBounds.Height / 2.0f;

			Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;
			Renderer.Drawing.FilledRectangle(new RectangleF(2, logoBounds.Y, Control.ClientSize.Width - 4, logoBounds.Height), Color.FromKnownColor(KnownColor.ControlDark));

			Renderer.Effects.GaussianBlur.BlurAmount = 3.2f;// _blurAmount;
			Renderer.Drawing.BlendingMode = BlendingMode.PreMultiplied;
			alpha = ((_blurAmount - 2.1f) / 15.8f);
			Renderer.Effects.GaussianBlur.Render((int pass) =>
				{
					if (pass == 0)
						Renderer.Drawing.FilledRectangle(new RectangleF(0, 0, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize.Width, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize.Height), new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f), _logo, new RectangleF(0, 0, logoSize.Width, logoSize.Height));
					else
						Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 0.5f - alpha), Renderer.Effects.GaussianBlur.BlurredTexture, new RectangleF(0, 0, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize.Width, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize.Height));
				});
			Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, alpha), _logo, new RectangleF(Vector2.Zero, _logo.Settings.Size));

			_blurAmount += timing.FrameDelta * _blurDelta;

			if ((_blurAmount < 2.1f) || (_blurAmount > 10.0f))
			{
				_blurDelta = -_blurDelta;
				_blurAmount += _blurDelta * timing.FrameDelta;
			}

			// Sleep.
			System.Threading.Thread.Sleep(3);
		}

		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected virtual void LoadGraphics()
		{
			Renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(64, 64);
			_logo = Program.Graphics.Textures.FromGDIBitmap("Logo", Properties.Resources.Gorgon_2_x_Logo_Full);
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
		/// Function to call for rendering.
		/// </summary>
		/// <param name="timing">Timing data to pass to the method.</param>
		/// <returns>TRUE to continue running, FALSE to exit.</returns>
		public void RenderMethod(GorgonFrameRate timing)
		{
			if ((SwapChain == null) || (Control == null) || (Renderer == null))
				return;

			SwapChain.Clear(Control.BackColor);
			Draw(timing);
			Renderer.Render();
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
				Window = Control
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
			: base(name)
		{
			Tab = new TabPageEx(name);
			Control = new Panel();
			Control.BackColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
			Control.Dock = DockStyle.Fill;
			Tab.Controls.Add(Control);
			Tab.IsClosable = allowClose;
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
					if (Tab != null)
						Tab.Dispose();
				}

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
	}
}
