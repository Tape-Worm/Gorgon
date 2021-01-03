#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 20, 2016 11:17:52 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// An include file for a shader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this object to load in included external functions for a shader.  If the shader source contains an HLSL <c>#include</c> directive, the shader compiler will try to locate that include file on 
    /// the file system.  However, this does not work when the files are loaded from a <see cref="Stream"/> (it wouldn't know where to find the include file).  So to facilitate this, this object will 
    /// contain the source for the include file and will be looked up <i>before</i> the file system is checked for the include file.
    /// </para>
    /// <para>
    /// Gorgon uses a special keyword in shaders to allow shader files to include other files as part of the source. This keyword is named <c>#GorgonInclude</c> and is similar to the HLSL 
    /// <c>#include</c> keyword. The difference is that this keyword allows users to include shader source from memory instead of a separate source file. This is done by assigning a name to the included 
    /// source code in the <c>#GorgonInclude</c> keyword, and adding the <see cref="GorgonShaderInclude"/> containing the source to the <see cref="GorgonShaderFactory.Includes"/> property on the 
    /// <see cref="GorgonShaderFactory"/> class. When the include is loaded from a file, then it will automatically be added to the <see cref="GorgonShaderFactory.Includes"/> property.
    /// </para>
    /// <para>
    /// The parameters for the <c>#GorgonInclude</c>keyword are: 
    /// <list type="bullet">
    ///		<item>
    ///			<term>Name</term>
    ///			<description>The path name of the included source. This must be unique, and assigned to the <see cref="GorgonShaderFactory.Includes"/> property.</description>
    ///		</item>
    ///		<item>
    ///			<term>(Optional) Path</term>
    ///			<description>The path to the shader source file to include. This may be omitted if the include is assigned from memory in the <see cref="GorgonShaderFactory.Includes"/> property.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// <para>
    /// These include objects are ignored with binary shader data loaded from a <see cref="Stream"/> or file as those will aready contain the included source compiled into bytecode.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShaderFactory"/>
    public readonly struct GorgonShaderInclude
        : IGorgonNamedObject, IEquatable<GorgonShaderInclude>
    {
        #region Variables.
        /// <summary>
        /// The name of the shader include file.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The source code for the shader include file.
        /// </summary>
        public readonly string SourceCodeFile;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the name of the include file.
        /// </summary>
        string IGorgonNamedObject.Name => Name;
        #endregion

        #region Methods.
        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GORGFX_TOSTR_SHADER_INCLUDE, Name);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// Function to evaluate two instances for equality.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(in GorgonShaderInclude left, in GorgonShaderInclude right) => (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj) => obj is GorgonShaderInclude include ? include.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonShaderInclude left, GorgonShaderInclude right) => Equals(left, right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonShaderInclude left, GorgonShaderInclude right) => !Equals(left, right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GorgonShaderInclude other) => Equals(this, other);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderInclude"/> struct.
        /// </summary>
        /// <param name="includeName">Name of the include file.</param>
        /// <param name="includeSourceFile">The include source code file.</param>
        /// <remarks>The <paramref name="includeSourceFile"/> can be set to <b>null</b> or empty if the include line is pointing to a file.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="includeName"/> parameters is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the includeName parameter is empty.</exception>
        public GorgonShaderInclude(string includeName, string includeSourceFile)
        {
            if (includeName == null)
            {
                throw new ArgumentNullException(nameof(includeName));
            }

            if (string.IsNullOrWhiteSpace(includeName))
            {
                throw new ArgumentEmptyException(nameof(includeName));
            }

            if (includeSourceFile == null)
            {
                includeSourceFile = string.Empty;
            }

            Name = includeName;
            SourceCodeFile = includeSourceFile;
        }
        #endregion
    }
}
