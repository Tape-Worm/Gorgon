#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 25, 2018 10:21:07 PM
// 
#endregion

using System;
using Gorgon.Animation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gorgon.IO
{
    /// <summary>
    /// Extension methods for IO on animation objects.
    /// </summary>
    public static class GorgonAnimationExtensions
    {
        #region Constants.
        /// <summary>
        /// The property name for the header value.
        /// </summary>
        internal const string JsonHeaderProp = "header";
        /// <summary>
        /// The property name for the header value.
        /// </summary>
        internal const string JsonVersionProp = "version";
        #endregion

        #region Methods.
        /// <summary>
        /// Function to convert a <see cref="IGorgonAnimation"/> to a JSON string.
        /// </summary>
        /// <param name="animation">The animation to convert.</param>
        /// <param name="prettyFormat">[Optional] <b>true</b> to use pretty formatting for the string, or <b>false</b> to use a compact form.</param>
        /// <returns>The animation encoded as a JSON string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is <b>null</b>.</exception>
        /// <seealso cref="IGorgonAnimation"/>
        public static string ToJson(this IGorgonAnimation animation, bool prettyFormat = false)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            var settings = new JsonSerializer
            {
                Formatting = prettyFormat ? Formatting.Indented : Formatting.None,
                Converters =
                               {
                                   new JsonTextureKeyConverter(null),
                                   new JsonSingleKeyConverter(),
                                   new JsonVector2KeyConverter(),
                                   new JsonVector3KeyConverter(),
                                   new JsonVector4KeyConverter(),
                                   new JsonGorgonColorKeyConverter(),
                                   new JsonRectKeyConverter()
                               }
            };

            var jsonObj = JObject.FromObject(animation, settings);
            JToken firstProp = jsonObj.First;
            firstProp.AddBeforeSelf(new JProperty(JsonHeaderProp, GorgonAnimationCodecCommon.CurrentFileHeader));
            firstProp.AddBeforeSelf(new JProperty(JsonVersionProp, GorgonAnimationCodecCommon.CurrentVersion.ToString(2)));

            return jsonObj.ToString(prettyFormat ? Formatting.Indented : Formatting.None);
        }
        #endregion
    }
}
