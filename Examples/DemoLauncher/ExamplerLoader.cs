using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Gorgon.Examples
{
	/// <summary>
	/// Loader for the example XML manifest.
	/// </summary>
	static class ExampleLoader
	{
		/// <summary>
		/// Function to load a list of examples from the embedded example XML file.
		/// </summary>
		/// <param name="category">Category for the examples.</param>
		/// <param name="examples">XML nodes containing the examples.</param>
		/// <returns>A new example collection.</returns>
		public static IReadOnlyList<Example> Read(string category, IEnumerable<XElement> examples)
		{
			var result = new List<Example>();

			int index = 0;

			foreach (XElement exampleNode in examples)
			{
				Example example = Example.Read(index, category, exampleNode);

				if (((string.IsNullOrEmpty(example.ExePath)))
					|| (string.IsNullOrEmpty(example.Name))
					|| (result.Any(item => string.Equals(item.Name, example.Name, StringComparison.OrdinalIgnoreCase))))
				{
					example.Dispose();
					continue;
				}

				index++;
				result.Add(example);
			}

			return result;
		}
	}
}
