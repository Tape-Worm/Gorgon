#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Monday, August 19, 2013 11:25:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A compositor system that allows for the creation of specialized effects through shaders and various states.
    /// </summary>
    /// <remarks>
    /// An effect allows for the creation of various special effects when rendering a scene or an object.  The effect can use render states and/or shaders to achieve the 
    /// desired result.  
    /// <para>Effects can use a single or multiple passes to achieve its goal.  When multiple passes are utilized, the same scene may be rendered again (or a completely different one 
    /// depending on the desired effect) and composited into a final output.  The GorgonEffect object allows users to define passes as a set of <see cref="System.Action{T}"/> methods that 
    /// will be responsible for rendering the pass.</para>
    /// <para>This effect system is similar to that of the Direct3D HLSL effect system.</para>
    /// </remarks>
    public abstract class GorgonEffect
        : GorgonNamedObject, IDisposable
    {
        #region Variables.
        private bool _disposed;                         // Flag to indicate that the object was disposed.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return a pass containing the original shaders before rendering started.
		/// </summary>
	    protected GorgonEffectPass StoredShaders
	    {
		    get;
		    private set;
	    }

        /// <summary>
        /// Property to return a list of required parameters for the effect.
        /// </summary>
        /// <remarks>These parameters are required upon creation of the effect.  Developers implementing a new effect object 
        /// requiring data to be sent to a shader at creation should fill in this list with the names of the required 
        /// parameters.</remarks>
        protected internal IList<string> RequiredParameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a list of additional parameters for the effect.
        /// </summary>
        /// <remarks>These parameters can be passed in via construction of the effect using the <see cref="GorgonLibrary.Graphics.GorgonShaderBinding.CreateEffect{T}">CreateEffect</see> method 
        /// or they may be updated after the object was created.
        /// </remarks>
        public IDictionary<string, object> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the graphics interface that created this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the list of passes for this effect.
        /// </summary>
        public GorgonEffectPassArray Passes
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to remember the currently set shaders.
		/// </summary>
	    protected void RememberCurrentShaders()
	    {
			StoredShaders.PixelShader = Graphics.Shaders.PixelShader.Current;
			StoredShaders.VertexShader = Graphics.Shaders.VertexShader.Current;

			if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
			{
				return;
			}

			StoredShaders.GeometryShader = Graphics.Shaders.GeometryShader.Current;

			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				return;
			}

			StoredShaders.ComputeShader = Graphics.Shaders.ComputeShader.Current;
			StoredShaders.HullShader = Graphics.Shaders.HullShader.Current;
			StoredShaders.DomainShader = Graphics.Shaders.DomainShader.Current;
	    }

		/// <summary>
		/// Function to restore the previously remembered shaders.
		/// </summary>
	    protected void RestoreCurrentShaders()
	    {
			Graphics.Shaders.PixelShader.Current = StoredShaders.PixelShader;
			Graphics.Shaders.VertexShader.Current = StoredShaders.VertexShader;

			if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
			{
				return;
			}

			Graphics.Shaders.GeometryShader.Current = StoredShaders.GeometryShader;

			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				return;
			}

			Graphics.Shaders.ComputeShader.Current = StoredShaders.ComputeShader;
			Graphics.Shaders.HullShader.Current = StoredShaders.HullShader;
			Graphics.Shaders.DomainShader.Current = StoredShaders.DomainShader;
	    }

        /// <summary>
        /// Function called before rendering begins.
        /// </summary>
        /// <returns>TRUE to continue rendering, FALSE to stop.</returns>
        protected virtual bool OnBeforeRender()
        {
            return true;
        }

        /// <summary>
        /// Function called after rendering ends.
        /// </summary>
        protected virtual void OnAfterRender()
        {
        }

        /// <summary>
        /// Function called before a pass is rendered.
        /// </summary>
        /// <param name="pass">Pass to render.</param>
        /// <returns>TRUE to continue rendering, FALSE to stop.</returns>
        protected virtual bool OnBeforePassRender(GorgonEffectPass pass)
        {
			RememberCurrentShaders();

			Graphics.Shaders.PixelShader.Current = pass.PixelShader;
			Graphics.Shaders.VertexShader.Current = pass.VertexShader;

			if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
			{
				return true;
			}

			Graphics.Shaders.GeometryShader.Current = pass.GeometryShader;

			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				return true;
			}

			Graphics.Shaders.ComputeShader.Current = pass.ComputeShader;
			Graphics.Shaders.HullShader.Current = pass.HullShader;
			Graphics.Shaders.DomainShader.Current = pass.DomainShader;

			return true;
        }

        /// <summary>
        /// Function called after a pass has been rendered.
        /// </summary>
        /// <param name="pass">Pass that was rendered.</param>
        protected virtual void OnAfterPassRender(GorgonEffectPass pass)
        {
            RestoreCurrentShaders();
        }

		/// <summary>
		/// Function to render a pass.
		/// </summary>
		/// <param name="pass">Pass that is to be rendered.</param>
	    protected virtual void OnRenderPass(GorgonEffectPass pass)
	    {
			if (pass.RenderAction != null)
			{
				pass.RenderAction(pass);
			}
	    }

        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
        protected virtual void OnInitialize()
        {
            
        }

        /// <summary>
        /// Function to initialize the effect.
        /// </summary>
        /// <param name="parameters">The parameters used to initialize the effect.</param>
        internal void InitializeEffect(GorgonEffectParameter[] parameters)
        {
            // Check its required parameters.
            if ((RequiredParameters.Count > 0) && ((parameters == null) || (parameters.Length == 0)))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_EFFECT_MISSING_REQUIRED_PARAMS, RequiredParameters[0]), "parameters");
            }

            if ((parameters != null) && (parameters.Length > 0))
            {
                // Only get parameters where the key name has a value.
                var validParameters = parameters.Where(item => !string.IsNullOrWhiteSpace(item.Name)).ToArray();

                // Check for predefined required parameters from the effect.
                var missingParams =
                    RequiredParameters.Where(
                        effectParam =>
                            (!validParameters.Any(
                                item => string.Equals(item.Name, effectParam, StringComparison.OrdinalIgnoreCase))));

                StringBuilder missingList = null;
                foreach (var effectParam in missingParams)
                {
                    if (missingList == null)
                    {
                        missingList = new StringBuilder(128);
                    }

                    if (missingList.Length > 0)
                    {
                        missingList.Append("\n");
                    }

                    missingList.AppendFormat("'{0}'", effectParam);
                }

                if ((missingList != null)
                    && (missingList.Length > 0))
                {
                    throw new ArgumentException(string.Format(Resources.GORGFX_EFFECT_MISSING_REQUIRED_PARAMS, missingList), "parameters");
                }

                // Add/update the parameters.
                foreach (var param in validParameters)
                {
                    Parameters[param.Name] = param.Value;
                }
            }

            OnInitialize();
        }

        /// <summary>
        /// Function to render a single pass.
        /// </summary>
        /// <param name="passIndex">Index of the pass to render.</param>
        public void RenderPass(int passIndex)
        {
            if (!OnBeforeRender())
            {
                return;
            }

            var pass = Passes[passIndex];

            if (OnBeforePassRender(pass))
            {
                OnRenderPass(pass);
            }

            OnAfterPassRender(pass);

            OnAfterRender();
        }

        /// <summary>
        /// Function to render all passes.
        /// </summary>
        public void Render()
        {
            if (!OnBeforeRender())
            {
                return;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < Passes.Count; i++)
            {
                var pass = Passes[i];

                if (OnBeforePassRender(pass))
                {
                    OnRenderPass(pass);
                }

                OnAfterPassRender(pass);
            }

            OnAfterRender();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonEffect"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that constructed this effect.</param>
        /// <param name="name">The name of the effect.</param>
        /// <param name="passCount">The pass count.</param>
        protected GorgonEffect(GorgonGraphics graphics, string name, int passCount)
            : base(name)
        {
            Graphics = graphics;
            Passes = new GorgonEffectPassArray(this, passCount);
            StoredShaders = new GorgonEffectPass(this, -1);
	        RequiredParameters = new List<string>();
			Parameters = new Dictionary<string, object>();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Graphics.RemoveTrackedObject(this);
            }

            _disposed = true;
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
