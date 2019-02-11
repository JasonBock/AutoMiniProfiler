using System.Threading;

namespace AutoMiniProfiler.Playground
{
	public sealed class LongRunningService
	{
		public void Run()
		{
			using (TimingCreator.GetTiming("LongRunningService.Run"))
			{
				Thread.Sleep(1500);
			}
		}
	}
}
