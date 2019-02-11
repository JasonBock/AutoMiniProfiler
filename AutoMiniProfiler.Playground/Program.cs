using AutoMiniProfiler.Core;
using Microsoft.CodeAnalysis.CSharp;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoMiniProfiler.Playground
{
	class Program
	{
		static void Main(string[] args)
		{
			//await Program.DoManualTimings();
			var code =
@"using System;

class C 
{ 
  void Bar() => Console.Out.WriteLine(""bar"");

  int Foo()
  {
    var x = 42;
    var y = 22;
    return x + y;
  }
}";
			Console.Out.WriteLine("Before...");
			Console.Out.WriteLine(code);
			Console.Out.WriteLine();

			var unit = SyntaxFactory.ParseCompilationUnit(code);
			var tree = unit.SyntaxTree;
			var x = CSharpCompilation.Create("a",
				syntaxTrees: new[] { tree });

			var modified = new MethodProfilerInjectionRewriter(x.GetSemanticModel(tree)).Visit(tree.GetRoot());
			Console.Out.WriteLine("After...");
			Console.Out.WriteLine(modified.GetText());
		}

		private static async Task DoManualTimings()
		{
			using (TimingCreator.GetTiming("Program.Main"))
			{
				new ShortToMediumToLongConsumer().Run();
			}

			var profiler = MiniProfiler.Current;
			var hierarchy = profiler.GetTimingHierarchy().ToList();

			await Program.Print(hierarchy);

			await Console.Out.WriteLineAsync();

			//await PrintGroupings(hierarchy);
		}

		private static async Task PrintGroupings(List<Timing> hierarchy)
		{
			var hierarchyGroups = hierarchy
				 .GroupBy(_ => _.Name)
				 .Select(_ => new { Name = _.Key, Average = TimeSpan.FromMilliseconds((double)_.Average(t => t.DurationMilliseconds)) });

			foreach (var timingGroup in hierarchyGroups)
			{
				await Console.Out.WriteLineAsync($"{timingGroup.Name} - {timingGroup.Average}");
			}
		}

		private static async Task Print(List<Timing> hierarchy)
		{
			async Task Print(Timing timing, int indent)
			{
				await Console.Out.WriteLineAsync(
					$"{new string(' ', indent)}{timing.Name} - {TimeSpan.FromMilliseconds((double)timing.DurationMilliseconds)}");

				if(timing.HasChildren)
				{
					foreach (var childTiming in timing.Children)
					{
						await Print(childTiming, indent + 2);
					}
				}
			}

			foreach (var timing in hierarchy)
			{
				if (timing.IsRoot) { await Print(timing, 0); }
			}
		}
	}
}
