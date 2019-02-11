using System.Threading;

namespace AutoMiniProfiler.Playground
{
	public sealed class MediumRunningService
	{
		public void Run()
		{
			using (TimingCreator.GetTiming("MediumRunningService.Run"))
			{
				Thread.Sleep(500);
			}
		}
	}
}
