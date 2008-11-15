#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, April 01, 2008 9:33:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.FileSystems.Design
{
    /// <summary>
    /// Form used to present authentication to the file system editor.
    /// </summary>
    public partial class formAuthentication 
        : Form
    {
        #region Variables.
        private IAuthData _authorizationData = null;        // Authorization data.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the authorization data object.
        /// </summary>
        [Browsable(false)]
        public IAuthData AuthorizationData
        {
            get
            {
                return _authorizationData;
            }
            set
            {
                _authorizationData = value;
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="formAuthentication"/> class.
        /// </summary>
        public formAuthentication()
        {
            InitializeComponent();
        }
        #endregion
    }
}
