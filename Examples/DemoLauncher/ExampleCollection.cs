using System.Collections.Generic;
using System.Xml.Linq;
using Gorgon.Collections;

namespace Gorgon.Examples
{
	class ExampleCollection
		: GorgonBaseNamedObjectDictionary<Example>
	{
		#region Properties.
		/// <summary>
		/// Property to return the category for the examples.
		/// </summary>
		public string Category
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load a list of examples from the embedded example XML file.
		/// </summary>
		/// <param name="category">Category for the examples.</param>
		/// <param name="examples">XML nodes containing the examples.</param>
		/// <returns>A new example collection.</returns>
		public static ExampleCollection Read(string category, IEnumerable<XElement> examples)
		{
			var result = new ExampleCollection
			             {
				             Category = category
			             };

			int index = 0;

			foreach (XElement exampleNode in examples)
			{
				Example example = Example.Read(index, category, exampleNode);

				if (((string.IsNullOrEmpty(example.ExePath)))
					|| (string.IsNullOrEmpty(example.Name))
					|| (result.Contains(example.Name)))
				{
					example.Dispose();
					continue;
				}

				index++;
				result.Add(example);
			}

			return result;
		}

		/// <summary>
		/// Function to add an example to the collection,
		/// </summary>
		/// <param name="example">The example to add.</param>
		private void Add(Example example)
		{
			AddItem(example);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ExampleCollection"/> class.
		/// </summary>
		private ExampleCollection()
			: base(false)
		{
		}
		#endregion
	}
}
