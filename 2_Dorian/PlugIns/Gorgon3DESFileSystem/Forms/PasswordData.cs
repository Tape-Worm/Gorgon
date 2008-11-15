using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object used to get the password from the user.
    /// </summary>
    public class PasswordData
        : IAuthData
    {
        #region Variables.
        private byte[] _data = null;            // Password data.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set the password.
        /// </summary>
        /// <param name="password">Password to set.</param>
        public void SetPassword(string password)
        {
            if (password.Length < 6)
                throw new Exception("Password length must be at least 6 characters.");

            // Use a maximum of 24 characters.
            if (password.Length > 24)
                password = password.Substring(0, 24);

            _data = null;

            if (string.IsNullOrEmpty(password))
                return;

            _data = Encoding.UTF8.GetBytes(password);
        }
        #endregion

        #region IAuthData Members
        /// <summary>
        /// Property to set or return the authentication data.
        /// </summary>
        /// <value></value>
        public byte[] Data
        {
            get
            {
                return _data;
            }
        }
        #endregion
    }
}
