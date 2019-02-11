namespace AutoMiniProfiler.Playground
{
	public sealed class ShortToMediumToLongConsumer
	{
		public void Run()
		{
			using (TimingCreator.GetTiming("ShortToMediumToLongConsumer.Run"))
			{
				new ShortRunningService().Run();
				this.RunMediumAndLong();
			}
		}

		private void RunMediumAndLong()
		{
			using (TimingCreator.GetTiming("ShortToMediumToLongConsumer.RunMediumAndLong"))
			{
				new MediumRunningService().Run();
				this.RunLong();
			}
		}

		private void RunLong()
		{
			using (TimingCreator.GetTiming("ShortToMediumToLongConsumer.RunLong"))
			{
				new LongRunningService().Run();
			}
		}
	}
}