using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D12 = SharpDX.Direct3D12;
using Gorgon.Core;

namespace Gorgon.Graphics
{
	public class TestClass
		: IDisposable
	{
		private GorgonSwapChain _swapChain;
		private long _fencey = 0;

		public void DoStuff()
		{
			//_swapChain.Graphics.GraphicsCommander.WaitForFence(_fencey);

			_swapChain.Clear(new GorgonColor(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), 1));

			_swapChain.Present(0);

			//if (!_swapChain.Graphics.GraphicsCommander.IsFenceComplete(fence))
			{
				//Debug.Print($"Target index: {_targetIndex}");
				
			}
			//;
			//
		}

		public void Dispose()
		{
		}

		public void Prepare()
		{
			/*D3D12.GraphicsCommandList list = _list[_targetIndex];

			long fence = _swapChain.Graphics.GraphicsCommander.ExecuteCommandList(list);
			_swapChain.Graphics.GraphicsCommander.Retire(fence, _allocator[_targetIndex]);
			long currentValue = _fenceTicks[_targetIndex];

			_graphics.CommandQueue.Signal(_fence, currentValue);

			_targetIndex = _swap.CurrentBackBufferIndex;

			if (_fence.CompletedValue < _fenceTicks[_targetIndex])
			{
				_fence.SetEventOnCompletion(_fenceTicks[_targetIndex], _frameSyncEvent.SafeWaitHandle.DangerousGetHandle());
				_frameSyncEvent.WaitOne();
			}

			_fenceTicks[_targetIndex] = currentValue + 1;*/
		}

		public TestClass(GorgonSwapChain swapChain)
		{
			_swapChain = swapChain;
		}
	}
}
