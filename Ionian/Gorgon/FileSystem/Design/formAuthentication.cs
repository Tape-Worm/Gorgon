#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
