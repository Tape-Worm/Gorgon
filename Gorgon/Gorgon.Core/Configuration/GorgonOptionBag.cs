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
// Created: June 29, 2016 8:13:58 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Configuration
{
	/// <summary>
	/// Provides a functionality for setting and reading various options from a predefined option bag.
	/// </summary>
	public sealed class GorgonOptionBag
		: IGorgonOptionBag
	{
		#region Variables.
		// The type information for each option.
		private readonly Dictionary<string, Type> _optionTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		// The values for each option.
		private readonly Dictionary<string, object> _optionValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		// The default values for each option.
		private readonly Dictionary<string, object> _defaultValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of available option names and their expected types.
		/// </summary>
		public IReadOnlyDictionary<string, Type> OptionKeys => _optionTypes;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the value for an option.
		/// </summary>
		/// <typeparam name="T">The type of data for the option.</typeparam>
		/// <param name="optionName">The name of the option.</param>
		/// <returns>The value stored with the option.</returns>
		public T GetOption<T>(string optionName)
		{
			object value = _optionValues[optionName];

			// If the value is null, then try to return its default.
			if (value == null)
			{
				return (T)_defaultValues[optionName];
			}

			return (T)value;
		}

		/// <summary>
		/// Function to assign a value for an option.
		/// </summary>
		/// <typeparam name="T">The type of data for the option.</typeparam>
		/// <param name="optionName">The name of the option.</param>
		/// <param name="value">The value to assign to the option.</param>
		public void SetOption<T>(string optionName, T value)
		{
			if (!_optionValues.ContainsKey(optionName))
			{
				throw new KeyNotFoundException();
			}

			_optionValues[optionName] = value;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOptionBag"/> class.
		/// </summary>
		/// <param name="options">Values and types used to initialize the options..</param>
		public GorgonOptionBag(IEnumerable<KeyValuePair<string, Tuple<object, Type>>>  options)
		{
			foreach (KeyValuePair<string, Tuple<object, Type>> option in options)
			{
				_defaultValues[option.Key] = _optionValues[option.Key] = option.Value.Item1;
				_optionTypes[option.Key] = option.Value.Item2;
			}
		}
		#endregion
	}
}
