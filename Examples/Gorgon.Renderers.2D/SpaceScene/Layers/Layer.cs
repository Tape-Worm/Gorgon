#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 20, 2019 11:43:44 PM
// 
#endregion

using System.Collections.Generic;
using System.Numerics;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// A class that represnets a layer that can be rendered into the scene.
    /// </summary>
    internal abstract class Layer
    {
        #region Variables.
        // The list of active lights for the layer.
        private readonly List<Light> _activeLights = new();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of lights that are used to light this layer.
        /// </summary>
        protected IList<Light> ActiveLights => _activeLights;

        /// <summary>
        /// Property to return the size of the target that this layer will be rendered into.
        /// </summary>
        protected DX.Size2 OutputSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the lights owned by this layer.
        /// </summary>
        /// <remarks>
        /// Lights owned by the layer only affect their transformation (for point lights), the <see cref="Light.Layers"/> list is used to determine which lights are used to render.
        /// </remarks>
        public IList<Light> Lights
        {
            get;
        } = new List<Light>();

        /// <summary>
        /// Property to set or return the post processing group that the layer belongs to.
        /// </summary>
        public string PostProcessGroup
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the offset for the layer.
        /// </summary>
        public Vector2 Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the level of parallax for this layer.
        /// </summary>
        public float ParallaxLevel
        {
            get;
            set;
        } = 1.0f;

        /// <summary>
        /// Property to set or return the camera to be used with this layer.
        /// </summary>
        public GorgonCameraCommon Camera
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the light transforms for this layer.
        /// </summary>
        protected void UpdateLightTransforms()
        {
            // Update all lights.
            for (int i = 0; i < Lights.Count; ++i)
            {
                Light light = Lights[i];

                if (light is null)
                {
                    continue;
                }

                if (light.PointLight is not null)
                {
                    Vector3 newPosition = (light.LocalLightPosition + new Vector3(Offset, 0)) / ParallaxLevel;
                    light.PointLight.Position = new Vector3(new Vector2(newPosition.X, newPosition.Y), Lights[i].LocalLightPosition.Z);
                }
            }
        }

        /// <summary>
        /// Function called to update items per frame on the layer.
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// Function called when the layer is resized.
        /// </summary>
        protected virtual void OnResized()
        {
        }

        /// <summary>
        /// Function to apply the specified light to the layer.
        /// </summary>
        /// <param name="light">The light to apply.</param>
        public void ApplyLight(Light light)
        {
            if (light is null)
            {
                return;
            }

            if (_activeLights.Contains(light))
            {
                return;
            }

            _activeLights.Add(light);
        }

        /// <summary>
        /// Function to clear the active lights from the layer.
        /// </summary>
        public void ClearActiveLights() => _activeLights.Clear();

        /// <summary>
        /// Function called when our layer needs resizing.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        public void OnResize(DX.Size2 newSize)
        {
            OutputSize = newSize;
            OnResized();
        }

        /// <summary>
        /// Function called to update items per frame on the layer.
        /// </summary>
        public void Update()
        {
            UpdateLightTransforms();
            OnUpdate();
        }

        /// <summary>
        /// Function used to load in resources required by the layer.
        /// </summary>
        public virtual void LoadResources()
        {

        }

        /// <summary>
        /// Function used to render data into the layer.
        /// </summary>
        public abstract void Render();
        #endregion
    }
}
