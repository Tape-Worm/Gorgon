#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: February 7, 2021 12:31:53 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// Configuration information for the example application.
    /// </summary>
    public class ExampleConfig
    {
        // The default instance.
        private static ExampleConfig _default;

        /// <summary>
        /// Function to load the configuration.
        /// </summary>
        private static void LoadConfig()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                            .Build();

            IConfigurationSection section = config.GetSection(nameof(ExampleConfig));

            _default = new ExampleConfig();

            if (section == null)
            {
                return;
            }

            section.Bind(_default);
        }

        /// <summary>
        /// Property to return the default settings.
        /// </summary>
        public static ExampleConfig Default
        {
            get
            {
                if (_default == null)
                {
                    LoadConfig();
                }

                return _default;
            }
        }

        /// <summary>
        /// Property to set or return the path to the resources for the example.
        /// </summary>
        public string ResourceLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the path to the location for example plug ins.
        /// </summary>
        public string PlugInLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the desired window resolution.
        /// </summary>
        public DX.Size2 Resolution
        {
            get;
            set;
        } = new DX.Size2(1280, 800);

        /// <summary>
        /// Property to set or return whether the example runs in windowed mode, or full screen mode.
        /// </summary>
        public bool IsWindowed
        {
            get;
            set;
        } = true;
    }
}
