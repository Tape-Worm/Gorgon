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
// Created: November 7, 2019 1:42:13 PM
// 
#endregion

using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// The concrete implementation of <see cref="IGorgon2DEffectBuilders"/>.
    /// </summary>
    internal class EffectBuilders
        : IGorgon2DEffectBuilders
    {
        /// <summary>Property to return the batch state builder.</summary>
        public Gorgon2DBatchStateBuilder BatchBuilder
        {
            get;
        } = new Gorgon2DBatchStateBuilder();

        /// <summary>Property to return the vertex shader state builder.</summary>
        public Gorgon2DShaderStateBuilder<GorgonVertexShader> VertexShaderBuilder
        {
            get;
        } = new Gorgon2DShaderStateBuilder<GorgonVertexShader>();

        /// <summary>Property to return the pixel shader state builder.</summary>
        public Gorgon2DShaderStateBuilder<GorgonPixelShader> PixelShaderBuilder
        {
            get;
        } = new Gorgon2DShaderStateBuilder<GorgonPixelShader>();
    }
}
