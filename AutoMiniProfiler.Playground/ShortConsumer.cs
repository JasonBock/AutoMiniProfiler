namespace AutoMiniProfiler.Playground
{
	public sealed class ShortConsumer
	{
		public void Run()
		{
			using (TimingCreator.GetTiming("ShortConsumer.Run"))
			{
				new ShortRunningService().Run();
			}
		}
	}
}
