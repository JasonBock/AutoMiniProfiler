using System.Threading;

namespace AutoMiniProfiler.Playground
{
	public sealed class ShortRunningService
	{
		public void Run()
		{
			using (TimingCreator.GetTiming("ShortRunningService.Run"))
			{
				Thread.Sleep(100);
			}
		}
	}
}
