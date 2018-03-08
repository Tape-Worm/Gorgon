using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace Font_EventLeak_Test
{
	class GorgonTextDebug
	{
		#region Variables.
		private List<WeakReference<GorgonText>> _objects = new List<WeakReference<GorgonText>>();
		#endregion

		#region Properties.
		public IList<WeakReference<GorgonText>> Objects
		{
			get
			{
				return _objects;
			}
		}
		#endregion

		#region Methods.
		public void GcTest()
		{
			GC.Collect(2, GCCollectionMode.Forced, true);
			GC.WaitForFullGCComplete();

			for (int i = 0; i < _objects.Count; ++i)
			{
				GorgonText text;

				if (_objects[i].TryGetTarget(out text))
				{
					throw new Exception(string.Format("Text object #{0} still alive. Probably still held by font.", i));
				}
			}

			_objects.Clear();
		}
		#endregion
	}
}
