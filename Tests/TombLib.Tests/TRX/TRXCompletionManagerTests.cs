using Nickelony.LanguageServer.Abstractions.Completion;
using System.Diagnostics;
using System.Globalization;
using TombLib.Scripting.Completion;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXCompletionManagerTests
{
	private static readonly IReadOnlyList<TextCompletionItem> Items =
	[
		new TextCompletionItem("\"title\": "),
		new TextCompletionItem("\"level\": "),
		new TextCompletionItem("true"),
		new TextCompletionItem("false"),
		new TextCompletionItem("null")
	];

	private static CompletionManager CreateManager()
		=> new(new TRXLineService());

	[TestMethod]
	public void FilterCompletions_EmptyWord_ReturnsAllItems()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, string.Empty);

		Assert.AreEqual(Items.Count, result.Count);
	}

	[TestMethod]
	public void FilterCompletions_QuoteOnlyWord_ReturnsAllItems()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "\"");

		Assert.AreEqual(Items.Count, result.Count);
	}

	[TestMethod]
	public void FilterCompletions_MatchesQuotedPrefixCaseInsensitively()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "\"Ti");

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("\"title\": ", result[0].InsertText);
	}

	[TestMethod]
	public void FilterCompletions_MatchesWordWithoutQuotes()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "lev");

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("\"level\": ", result[0].InsertText);
	}

	[TestMethod]
	public void FilterCompletions_NoMatch_ReturnsEmpty()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "zzz");

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void FilterCompletions_IsOrdinalCaseInsensitive_NotCultureSensitive()
	{
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

		try
		{
			// Under tr-TR, "TITLE".ToLower() is "tıtle" (dotless i), so a culture-sensitive
			// filter would fail to match "title"; ordinal ignore-case matching must still match.
			IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "\"TITLE");

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("\"title\": ", result[0].InsertText);
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	[TestMethod]
	public void FilterCompletions_PrefixMatchesSortBeforeContainsMatches()
	{
		IReadOnlyList<TextCompletionItem> customItems =
		[
			new TextCompletionItem("\"partition\": "),
			new TextCompletionItem("\"title\": ")
		];

		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(customItems, "\"ti");

		Assert.AreEqual(2, result.Count);
		Assert.AreEqual("\"title\": ", result[0].InsertText);
		Assert.AreEqual("\"partition\": ", result[1].InsertText);
	}

	[TestMethod]
	public void FilterCompletions_SingleStage_IsNoSlowerOrHeavierThanDoubleStage()
	{
		CompletionManager manager = CreateManager();
		var context = new TextCompletionContext("\"ti", 3);

		// The pre-Phase-19 path filtered in the provider (TextCompletionFilter) and again in the
		// coordinator (CompletionManager); the coordinator-owned single stage must not regress it.
		const int iterations = 500;

		// Warm up both code paths so JIT and static caches are settled.
		RunDoubleStageFilter(manager, context);
		RunSingleStageFilter(manager);

		long beforeAllocated = MeasureAllocations(() => RunDoubleStageFilter(manager, context), iterations);
		long afterAllocated = MeasureAllocations(() => RunSingleStageFilter(manager), iterations);
		double beforeElapsed = MeasureElapsed(() => RunDoubleStageFilter(manager, context), iterations);
		double afterElapsed = MeasureElapsed(() => RunSingleStageFilter(manager), iterations);

		Assert.IsTrue(afterAllocated <= beforeAllocated, $"Single stage allocated {afterAllocated} B vs {beforeAllocated} B.");
		Assert.IsTrue(afterElapsed <= beforeElapsed * 2.0, $"Single stage took {afterElapsed:F1} ms vs {beforeElapsed:F1} ms.");
	}

	private static IReadOnlyList<TextCompletionItem> RunSingleStageFilter(CompletionManager manager)
		=> manager.FilterCompletions(Items, "\"ti");

	private static IReadOnlyList<TextCompletionItem> RunDoubleStageFilter(CompletionManager manager, TextCompletionContext context)
	{
		IReadOnlyList<TextCompletionItem> providerFiltered = TextCompletionFilter.FilterByCurrentWord(Items, context);
		return manager.FilterCompletions(providerFiltered, "\"ti");
	}

	private static long MeasureAllocations(Action action, int iterations)
	{
		long before = GC.GetAllocatedBytesForCurrentThread();

		for (int i = 0; i < iterations; i++)
			action();

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	private static double MeasureElapsed(Action action, int iterations)
	{
		var stopwatch = Stopwatch.StartNew();

		for (int i = 0; i < iterations; i++)
			action();

		stopwatch.Stop();
		return stopwatch.Elapsed.TotalMilliseconds;
	}
}
