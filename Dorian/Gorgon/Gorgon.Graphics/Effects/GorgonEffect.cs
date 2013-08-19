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
        private GorgonEffectPass _storedShaders;        // List of stored shaders.
        #endregion

        #region Properties.
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
            _storedShaders.PixelShader = Graphics.Shaders.PixelShader.Current;
            _storedShaders.VertexShader = Graphics.Shaders.VertexShader.Current;
            _storedShaders.GeometryShader = Graphics.Shaders.GeometryShader.Current;
            _storedShaders.ComputeShader = Graphics.Shaders.ComputeShader.Current;
            _storedShaders.HullShader = Graphics.Shaders.HullShader.Current;
            _storedShaders.DomainShader = Graphics.Shaders.DomainShader.Current;

            pass.ApplyShaders();
            
            return true;
        }

        /// <summary>
        /// Function called after a pass has been rendered.
        /// </summary>
        /// <param name="pass">Pass that was rendered.</param>
        protected virtual void OnAfterPassRender(GorgonEffectPass pass)
        {
            _storedShaders.ApplyShaders();
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
                pass.Render();
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
                    pass.Render();
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
            _storedShaders = new GorgonEffectPass(this, -1);
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
