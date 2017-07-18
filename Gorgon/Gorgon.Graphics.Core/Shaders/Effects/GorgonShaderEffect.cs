#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 9, 2017 9:09:05 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The states to record and restore when rendering the effect.
    /// </summary>
    [Flags]
    public enum RecordStates
    {
        /// <summary>
        /// Don't record any states.
        /// </summary>
        None = 0,
        /// <summary>
        /// Record the current render targets and depth/stencil.
        /// </summary>
        RenderTargets = 1,
        /// <summary>
        /// Record the current view ports.
        /// </summary>
        Viewports = 2,
        /// <summary>
        /// Record the current scissor list.
        /// </summary>
        Scissors = 4,
        /// <summary>
        /// Record everything.
        /// </summary>
        All = RenderTargets | Viewports | Scissors
    }

    /// <summary>
    /// A shader effect used to simplify shader usage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows an application to set up a shader or multiple shaders (of differing types) with named parameters and other information. This type is to be passed to a <see cref="GorgonDrawCallBase"/> 
    /// and will be applied to the graphics pipeline from there.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// This is <b>not</b> the same as a HLSL effect (with techniques and passes).  This is just a means to simplify shader usage for an application. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public abstract class GorgonShaderEffect
        : GorgonNamedObject, IDisposable
    {
        #region Variables.
        // Flag to indicate whether or not the effect is in an initialized state.
        private bool _initialized;
        // The active render targets when rendering begins.
        private readonly GorgonRenderTargetView[] _activeRenderTargets;
        // The active viewports when rendering begins.
        private readonly GorgonMonitoredValueTypeArray<DX.ViewportF> _activeViewports;
        // The active scissor rectangles when rendering begins.
        private readonly GorgonMonitoredValueTypeArray<DX.Rectangle> _activeScissors;
        // The active depth/stencil view.
        private GorgonDepthStencilView _activeDepthStencil;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics interface that owns this effect.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the number of passes for the effect.
        /// </summary>
        public int PassCount
        {
            get;
        }

        /// <summary>
        /// Property to return which states this effect has recorded prior to rendering.
        /// </summary>
        /// <remarks>
        /// This will be assigned on a call to <see cref="Render"/> or <see cref="RecordStates"/>, and cleared after rendering, or on a call to <see cref="RestoreStates"/>.
        /// </remarks>
        public RecordStates RecordedStates
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the effect is initialized for the first time.
        /// </summary>
        protected abstract void OnInitialize();

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
        /// Function called before a rendering pass begins.
        /// </summary>
        /// <param name="passIndex">The current pass index.</param>
        /// <returns><b>true</b> to continue rendering, or <b>false</b> to skip this pass and move to the next.</returns>
        protected virtual bool OnBeforePass(int passIndex)
        {
            return true;
        }

        /// <summary>
        /// Function called after a rendering pass ends.
        /// </summary>
        /// <param name="passIndex">The current pass index.</param>
        protected virtual void OnAfterPass(int passIndex)
        {
        }

        /// <summary>
        /// Function called when a pass is rendered.
        /// </summary>
        /// <param name="passIndex">The current index of the pass being rendered.</param>
        /// <remarks>
        /// <para>
        /// Applications must override this method to provide data for rendering against the effect. 
        /// </para>
        /// </remarks>
        protected abstract void OnRenderPass(int passIndex);

        /// <summary>
        /// Function to initialize the effect.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            OnInitialize();

            _initialized = true;
        }

        /// <summary>
        /// Function to restore any recorded states.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method should be called after rendering the effect if the <see cref="RecordStates"/> method was called prior to rendering. If the <see cref="RecordedStates"/> property is set to 
        /// <see cref="Core.RecordStates.None"/> then this method will do nothing.
        /// </para>
        /// </remarks>
        /// <seealso cref="RecordStates"/>
        /// <seealso cref="RecordedStates"/>
        /// <seealso cref="Render"/>
        public void RestoreStates()
        {
            if (RecordedStates == Core.RecordStates.None)
            {
                return;
            }

            if ((RecordedStates & Core.RecordStates.RenderTargets) == Core.RecordStates.RenderTargets)
            {
                Graphics.SetRenderTargets(_activeRenderTargets, _activeDepthStencil);
            }

            if ((RecordedStates & Core.RecordStates.Viewports) == Core.RecordStates.Viewports)
            {
                ref (int Start, int Count, DX.ViewportF[] Viewports) viewports = ref _activeViewports.GetDirtyItems(true);

                for (int i = viewports.Start; i < viewports.Count + viewports.Start; ++i)
                {
                    viewports.Viewports[i] = _activeViewports[i];
                }
            }

            if ((RecordedStates & Core.RecordStates.Scissors) != Core.RecordStates.Scissors)
            {
                RecordedStates = Core.RecordStates.None;
                return;
            }

            ref (int Start, int Count, DX.Rectangle[] Scissors) scissors = ref _activeScissors.GetDirtyItems(true);

            for (int i = scissors.Start; i < scissors.Count + scissors.Start; ++i)
            {
                scissors.Scissors[i] = _activeScissors[i];
            }

            RecordedStates = Core.RecordStates.None;
        }

        /// <summary>
        /// Function to record the current state on the <see cref="GorgonGraphics"/> object that owns this effect.
        /// </summary>
        /// <param name="recordStates">The states to record.</param>
        /// <remarks>
        /// <para>
        /// This method will record the current states on the <see cref="GorgonGraphics"/> object prior to rendering the effect.  These can be restored with a call to the <see cref="RestoreStates"/> method.
        /// </para>
        /// <para>
        /// The <paramref name="recordStates"/> parameter is used to define which states to restore (or all states if desired) using the enumeration values as flags OR'd together. 
        /// </para>
        /// <para>
        /// If states were recorded before calling this method and <see cref="RestoreStates"/> was <b>not</b> called, then a subsequent call will automatically restore the states before recording.
        /// </para>
        /// <para>
        /// Keep in mind that the more states recorded, the more of a performance impact it will have.
        /// </para>
        /// </remarks>
        /// <seealso cref="RestoreStates"/>
        /// <seealso cref="RecordedStates"/>
        /// <seealso cref="Render"/>
        public void RecordStates(RecordStates recordStates)
        {
            if (recordStates == Core.RecordStates.None)
            {
                RecordedStates = Core.RecordStates.None;
                return;
            }

            // Undo the previous state recording.
            if (RecordedStates != Core.RecordStates.None)
            {
                RestoreStates();
            }

            if ((recordStates & Core.RecordStates.RenderTargets) == Core.RecordStates.RenderTargets)
            {
                _activeDepthStencil = Graphics.DepthStencilView;
                for (int i = 0; i < Graphics.RenderTargets.Count; ++i)
                {
                    _activeRenderTargets[i] = Graphics.RenderTargets[i];
                }
            }

            if ((recordStates & Core.RecordStates.Viewports) == Core.RecordStates.Viewports)
            {
                ref (int Start, int Count, DX.ViewportF[] Viewports) viewports = ref Graphics.Viewports.GetDirtyItems(true);

                for (int i = viewports.Start; i < viewports.Count + viewports.Start; ++i)
                {
                    _activeViewports[i] = viewports.Viewports[i];
                }
            }

            if ((recordStates & Core.RecordStates.Scissors) != Core.RecordStates.Scissors)
            {
                return;
            }

            ref (int Start, int Count, DX.Rectangle[] Scissors) scissors = ref Graphics.ScissorRectangles.GetDirtyItems(true);

            for (int i = scissors.Start; i < scissors.Count + scissors.Start; ++i)
            {
                _activeScissors[i] = scissors.Scissors[i];
            }
        }

        /// <summary>
        /// Function to render the effect using the defined passes.
        /// </summary>
        /// <param name="recordStates">[Optional] Flags to record states on the owning <see cref="Graphics"/> interface prior to rendering.</param>
        /// <remarks>
        /// <para>
        /// This will begin the process of rendering the effect. It will cycle through each registered pass and execute the code within. 
        /// </para>
        /// <para>
        /// The optional <paramref name="recordStates"/> flags are a combination of flags to indicate which states on the owning <see cref="Graphics"/> interface to record and restore prior to and after 
        /// rendering. If this parameter is omitted, then no state recording will occur. In some cases, users may desire more control over when the recording happens and when the restoration happens. 
        /// This can be facilitated through the <see cref="RecordStates"/> and <see cref="RestoreStates"/> methods.
        /// </para>
        /// <para>
        /// <note type="note">
        /// <para>
        /// Recording the states will incur a slight penalty to performance.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="RecordStates"/>
        /// <seealso cref="RestoreStates"/>
        public void Render(RecordStates recordStates = Core.RecordStates.None)
        {
            if (!_initialized)
            {
                throw new GorgonException(GorgonResult.NotInitialized, string.Format(Resources.GORGFX_ERR_EFFECT_NOT_INITIALIZED, Name));
            }

            if (!OnBeforeRender())
            {
                return;
            }

            bool hasRecordedStates = false;

            if (recordStates != Core.RecordStates.None)
            {
                RecordStates(recordStates);
                hasRecordedStates = true;
            }
            
            for (int i = 0; i < PassCount; ++i)
            {
                if (!OnBeforePass(i))
                {
                    continue;
                }

                OnRenderPass(i);

                OnAfterPass(i);
            }

            OnAfterRender();

            if (hasRecordedStates)
            {
                RestoreStates();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public abstract void Dispose();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderEffect"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this object.</param>
        /// <param name="name">The name of the effect.</param>
        /// <param name="passCount">The number of passes for this effect.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="passCount"/> parameter is less than 1.</exception>
        protected GorgonShaderEffect(GorgonGraphics graphics, string name, int passCount)
            : base(name)
        {
            if (passCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(passCount), Resources.GORGFX_ERR_EFFECT_PASS_COUNT_INVALID);
            }

            PassCount = passCount;
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _activeRenderTargets = new GorgonRenderTargetView[graphics.VideoDevice.MaxRenderTargetCount];
            _activeViewports = new GorgonMonitoredValueTypeArray<DX.ViewportF>(graphics.VideoDevice.MaxViewportCount);
            _activeScissors = new GorgonMonitoredValueTypeArray<DX.Rectangle>(graphics.VideoDevice.MaxScissorCount);
        }
        #endregion
    }
}
