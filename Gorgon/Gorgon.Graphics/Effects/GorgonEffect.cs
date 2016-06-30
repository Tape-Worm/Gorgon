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
using Gorgon.Core;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
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
        #region Value Types.
        /// <summary>
        /// A key used to track states for various shader types.
        /// </summary>
        private struct StateKey
            : IEquatable<StateKey>
        {
            private readonly ShaderType _shaderType;     // Type of shader.
            private readonly int _stateSlot;             // Slot to use for the shader.

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is StateKey)
                {
                    return ((StateKey)obj).Equals(this);
                }
                return base.Equals(obj);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(_shaderType).GenerateHash(_stateSlot);
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
            /// </returns>
            public bool Equals(StateKey other)
            {
                return _shaderType == other._shaderType && _stateSlot == other._stateSlot;
                }

            /// <summary>
            /// Initializes a new instance of the <see cref="StateKey"/> struct.
            /// </summary>
            /// <param name="shaderType">Type of the shader.</param>
            /// <param name="slot">The slot.</param>
            public StateKey(ShaderType shaderType, int slot)
            {
                _shaderType = shaderType;
                _stateSlot = slot;
            }
        }
        #endregion

        #region Variables.
        private bool _disposed;                                                     // Flag to indicate that the object was disposed.
        private Dictionary<StateKey, GorgonShaderView> _prevShaderViews;            // A list of previous shader views.
        private Dictionary<StateKey, GorgonConstantBuffer> _prevConstantBuffers;    // A list of previous constant buffers.
        private Dictionary<StateKey, GorgonTextureSamplerStates> _prevSamplers;     // A list of previous texture sampler states.
        private Dictionary<ShaderType, GorgonShader> _prevShaders;                  // A list of previous shaders.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of passes for this effect.
        /// </summary>
        protected GorgonEffectPassArray Passes
        {
            get;
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
        }

        /// <summary>
        /// Property to return a list of additional parameters for the effect.
        /// </summary>
        /// <remarks>These parameters can be passed in via construction of the effect using the <see cref="Gorgon.Graphics.GorgonShaderBinding.CreateEffect{T}">CreateEffect</see> method 
        /// or they may be updated after the object was created.
        /// </remarks>
        public IDictionary<string, object> Parameters
        {
            get;
        }

        /// <summary>
        /// Property to return the graphics interface that created this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to remember the previously assigned constant buffer for a given shader and slot index.
        /// </summary>
        /// <param name="shaderType">Type of the shader that the constant buffer belongs to.</param>
        /// <param name="slot">The slot for the constant buffer.</param>
        protected void RememberConstantBuffer(ShaderType shaderType, int slot)
        {
            var key = new StateKey(shaderType, slot);

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    _prevConstantBuffers[key] = Graphics.Shaders.VertexShader.ConstantBuffers[slot];
                    break;
                case ShaderType.Pixel:
                    _prevConstantBuffers[key] = Graphics.Shaders.PixelShader.ConstantBuffers[slot];
                    break;
                case ShaderType.Geometry:
                    _prevConstantBuffers[key] = Graphics.Shaders.GeometryShader.ConstantBuffers[slot];
                    break;
                case ShaderType.Compute:
                    _prevConstantBuffers[key] = Graphics.Shaders.ComputeShader.ConstantBuffers[slot];
                    break;
                case ShaderType.Domain:
                    _prevConstantBuffers[key] = Graphics.Shaders.DomainShader.ConstantBuffers[slot];
                    break;
                case ShaderType.Hull:
                    _prevConstantBuffers[key] = Graphics.Shaders.HullShader.ConstantBuffers[slot];
                    break;
            }
        }

        /// <summary>
        /// Function to remember the previously assigned shader resource for a given shader and slot index.
        /// </summary>
        /// <param name="shaderType">Type of the shader that the shader resource belongs to.</param>
        /// <param name="slot">The slot for the shader resource.</param>
        protected void RememberShaderResource(ShaderType shaderType, int slot)
        {
            var key = new StateKey(shaderType, slot);

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    _prevShaderViews[key] = Graphics.Shaders.VertexShader.Resources[slot];
                    break;
                case ShaderType.Pixel:
                    _prevShaderViews[key] = Graphics.Shaders.PixelShader.Resources[slot];
                    break;
                case ShaderType.Geometry:
                    _prevShaderViews[key] = Graphics.Shaders.GeometryShader.Resources[slot];
                    break;
                case ShaderType.Compute:
                    _prevShaderViews[key] = Graphics.Shaders.ComputeShader.Resources[slot];
                    break;
                case ShaderType.Domain:
                    _prevShaderViews[key] = Graphics.Shaders.DomainShader.Resources[slot];
                    break;
                case ShaderType.Hull:
                    _prevShaderViews[key] = Graphics.Shaders.HullShader.Resources[slot];
                    break;
            }
        }

        /// <summary>
        /// Function to remember the previously assigned texture sampler for a given shader and slot index.
        /// </summary>
        /// <param name="shaderType">Type of the shader that the texture sampler belongs to.</param>
        /// <param name="slot">The slot for the texture sampler.</param>
        protected void RememberTextureSampler(ShaderType shaderType, int slot)
        {
            var key = new StateKey(shaderType, slot);

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    _prevSamplers[key] = Graphics.Shaders.VertexShader.TextureSamplers[slot];
                    break;
                case ShaderType.Pixel:
                    _prevSamplers[key] = Graphics.Shaders.PixelShader.TextureSamplers[slot];
                    break;
                case ShaderType.Geometry:
                    _prevSamplers[key] = Graphics.Shaders.GeometryShader.TextureSamplers[slot];
                    break;
                case ShaderType.Compute:
                    _prevSamplers[key] = Graphics.Shaders.ComputeShader.TextureSamplers[slot];
                    break;
                case ShaderType.Domain:
                    _prevSamplers[key] = Graphics.Shaders.DomainShader.TextureSamplers[slot];
                    break;
                case ShaderType.Hull:
                    _prevSamplers[key] = Graphics.Shaders.HullShader.TextureSamplers[slot];
                    break;
            }
        }

        /// <summary>
        /// Function to restore a previously remembered constant buffer for a given shader type and slot index.
        /// </summary>
        /// <param name="shaderType">The type of shader.</param>
        /// <param name="slot">The index of the slot used to store the constant buffer.</param>
        protected void RestoreConstantBuffer(ShaderType shaderType, int slot)
        {
            GorgonConstantBuffer result;
            var key = new StateKey(shaderType, slot);

            if (!_prevConstantBuffers.TryGetValue(key, out result))
            {
                return;
            }

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    Graphics.Shaders.VertexShader.ConstantBuffers[slot] = result;
                    break;
                case ShaderType.Pixel:
                    Graphics.Shaders.PixelShader.ConstantBuffers[slot] = result;
                    break;
                case ShaderType.Geometry:
                    Graphics.Shaders.GeometryShader.ConstantBuffers[slot] = result;
                    break;
                case ShaderType.Compute:
                    Graphics.Shaders.ComputeShader.ConstantBuffers[slot] = result;
                    break;
                case ShaderType.Domain:
                    Graphics.Shaders.DomainShader.ConstantBuffers[slot] = result;
                    break;
                case ShaderType.Hull:
                    Graphics.Shaders.HullShader.ConstantBuffers[slot] = result;
                    break;
            }
        }

        /// <summary>
        /// Function to restore a previously remembered shader resource for a given shader type and slot index.
        /// </summary>
        /// <param name="shaderType">The type of shader.</param>
        /// <param name="slot">The index of the slot used to store the resource.</param>
        protected void RestoreShaderResource(ShaderType shaderType, int slot)
        {
            GorgonShaderView result;
            var key = new StateKey(shaderType, slot);

            if (!_prevShaderViews.TryGetValue(key, out result))
            {
                return;
            }

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    Graphics.Shaders.VertexShader.Resources[slot] = result;
                    break;
                case ShaderType.Pixel:
                    Graphics.Shaders.PixelShader.Resources[slot] = result;
                    break;
                case ShaderType.Geometry:
                    Graphics.Shaders.GeometryShader.Resources[slot] = result;
                    break;
                case ShaderType.Compute:
                    Graphics.Shaders.ComputeShader.Resources[slot] = result;
                    break;
                case ShaderType.Domain:
                    Graphics.Shaders.DomainShader.Resources[slot] = result;
                    break;
                case ShaderType.Hull:
                    Graphics.Shaders.HullShader.Resources[slot] = result;
                    break;
            }
        }

        /// <summary>
        /// Function to restore a previously remembered texture sampler for a given shader type and slot index.
        /// </summary>
        /// <param name="shaderType">The type of shader.</param>
        /// <param name="slot">The index of the slot used to store the texture sampler.</param>
        protected void RestoreTextureSampler(ShaderType shaderType, int slot)
        {
            GorgonTextureSamplerStates result;
            var key = new StateKey(shaderType, slot);

            if (!_prevSamplers.TryGetValue(key, out result))
            {
                return;
            }

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    Graphics.Shaders.VertexShader.TextureSamplers[slot] = result;
                    break;
                case ShaderType.Pixel:
                    Graphics.Shaders.PixelShader.TextureSamplers[slot] = result;
                    break;
                case ShaderType.Geometry:
                    Graphics.Shaders.GeometryShader.TextureSamplers[slot] = result;
                    break;
                case ShaderType.Compute:
                    Graphics.Shaders.ComputeShader.TextureSamplers[slot] = result;
                    break;
                case ShaderType.Domain:
                    Graphics.Shaders.DomainShader.TextureSamplers[slot] = result;
                    break;
                case ShaderType.Hull:
                    Graphics.Shaders.HullShader.TextureSamplers[slot] = result;
                    break;
            }
        }

        /// <summary>
        /// Function to remember the current shader for a given shader type.
        /// </summary>
        /// <param name="shaderType">Type of shader to remember.</param>
        protected void RememberCurrentShader(ShaderType shaderType)
        {
            switch (shaderType)
            {
                case ShaderType.Vertex:
                    _prevShaders[shaderType] = Graphics.Shaders.VertexShader.Current;
                    break;
                case ShaderType.Pixel:
                    _prevShaders[shaderType] = Graphics.Shaders.PixelShader.Current;
                    break;
                case ShaderType.Geometry:
                    _prevShaders[shaderType] = Graphics.Shaders.GeometryShader.Current;
                    break;
                case ShaderType.Compute:
                    _prevShaders[shaderType] = Graphics.Shaders.ComputeShader.Current;
                    break;
                case ShaderType.Domain:
                    _prevShaders[shaderType] = Graphics.Shaders.DomainShader.Current;
                    break;
                case ShaderType.Hull:
                    _prevShaders[shaderType] = Graphics.Shaders.HullShader.Current;
                    break;
            }
        }

        /// <summary>
        /// Function to restore the current shader for a given shader type.
        /// </summary>
        /// <param name="shaderType">Type of shader to remember.</param>
        protected void RestoreCurrentShader(ShaderType shaderType)
        {
            switch (shaderType)
            {
                case ShaderType.Vertex:
                    Graphics.Shaders.VertexShader.Current = (GorgonVertexShader)_prevShaders[shaderType];
                    break;
                case ShaderType.Pixel:
                    Graphics.Shaders.PixelShader.Current = (GorgonPixelShader)_prevShaders[shaderType];
                    break;
                case ShaderType.Geometry:
                    Graphics.Shaders.GeometryShader.Current = (GorgonGeometryShader)_prevShaders[shaderType];
                    break;
                case ShaderType.Compute:
                    Graphics.Shaders.ComputeShader.Current = (GorgonComputeShader)_prevShaders[shaderType];
                    break;
                case ShaderType.Domain:
                    Graphics.Shaders.DomainShader.Current = (GorgonDomainShader)_prevShaders[shaderType];
                    break;
                case ShaderType.Hull:
                    Graphics.Shaders.HullShader.Current = (GorgonHullShader)_prevShaders[shaderType];
                    break;
            }
        }

        /// <summary>
        /// Function called before rendering begins.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
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
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        protected abstract bool OnBeforePassRender(GorgonEffectPass pass);

        /// <summary>
        /// Function called after a pass has been rendered.
        /// </summary>
        /// <param name="pass">Pass that was rendered.</param>
        protected abstract void OnAfterPassRender(GorgonEffectPass pass);

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
                throw new ArgumentException(string.Format(Resources.GORGFX_EFFECT_MISSING_REQUIRED_PARAMS, RequiredParameters[0]), nameof(parameters));
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
                    throw new ArgumentException(string.Format(Resources.GORGFX_EFFECT_MISSING_REQUIRED_PARAMS, missingList), nameof(parameters));
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
	        RequiredParameters = new List<string>();
			Parameters = new Dictionary<string, object>();

            _prevConstantBuffers = new Dictionary<StateKey, GorgonConstantBuffer>();
            _prevSamplers = new Dictionary<StateKey, GorgonTextureSamplerStates>();
            _prevShaderViews = new Dictionary<StateKey, GorgonShaderView>();
            _prevShaders = new Dictionary<ShaderType, GorgonShader>();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
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
