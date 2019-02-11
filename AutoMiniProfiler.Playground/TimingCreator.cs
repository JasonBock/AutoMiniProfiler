using StackExchange.Profiling;
using System;

namespace AutoMiniProfiler.Playground
{
	internal static class TimingCreator
	{
		internal static IDisposable GetTiming(string method)
		{
			MiniProfiler profiler = default;
			var isRootProfiler = false;

			if (MiniProfiler.Current is null)
			{
				isRootProfiler = true;
				profiler = MiniProfiler.StartNew($"profile {method}");
			}
			else
			{
				profiler = MiniProfiler.Current;
			}

			return new TimingManager(profiler, profiler.Step(method), isRootProfiler);
		}

		private sealed class TimingManager
			: IDisposable
		{
			private readonly Timing timing;
			private readonly bool isRootProfiler;
			private readonly MiniProfiler profiler;

			public TimingManager(MiniProfiler profiler, Timing timing, bool isRootProfiler) =>
				(this.profiler, this.timing, this.isRootProfiler) = (profiler, timing, isRootProfiler);

			public void Dispose()
			{
				(this.timing as IDisposable).Dispose();
				if (this.isRootProfiler) { this.profiler.Stop(); }
			}
		}
	}
}