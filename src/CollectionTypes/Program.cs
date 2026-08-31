namespace CollectionTypes;

/// <summary>
/// Entry point. With no arguments, runs every demo in this project in order; pass one or more
/// demo names to run only those, or <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project CollectionTypes -- equality-mutable-key</c>.
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Demos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["list-growth"] = ListDemo.RunGrowthAsync,
        ["list-removal"] = ListDemo.RunRemovalAsync,
        ["list-mutation-during-enumeration"] = ListDemo.RunMutationDuringEnumerationAsync,
        ["list-sort-and-search"] = ListDemo.RunSortAndSearchAsync,
        ["list-as-span"] = ListDemo.RunAsSpanAsync,
        ["dictionary-lookup"] = DictionaryDemo.RunLookupAsync,
        ["dictionary-ordering"] = DictionaryDemo.RunOrderingAsync,
        ["dictionary-ref-access"] = DictionaryDemo.RunRefAccessAsync,
        ["dictionary-alternate-lookup"] = DictionaryDemo.RunAlternateLookupAsync,
        ["set-operations"] = SetDemo.RunOperationsAsync,
        ["set-relations"] = SetDemo.RunRelationsAsync,
        ["sorted-set"] = SetDemo.RunSortedSetAsync,
        ["queue-and-stack"] = OrderedAndSpecializedDemo.RunQueueAndStackAsync,
        ["linked-list"] = OrderedAndSpecializedDemo.RunLinkedListAsync,
        ["priority-queue"] = OrderedAndSpecializedDemo.RunPriorityQueueAsync,
        ["sorted-dictionary-vs-sorted-list"] = OrderedAndSpecializedDemo.RunSortedDictionaryVsSortedListAsync,
        ["equality-default"] = EqualityComparerDemo.RunDefaultAsync,
        ["equality-custom-comparer"] = EqualityComparerDemo.RunCustomComparerAsync,
        ["equality-mutable-key"] = EqualityComparerDemo.RunMutableKeyAsync,
        ["readonly-view-is-not-immutable"] = ImmutableAndFrozenDemo.RunReadOnlyViewAsync,
        ["immutable-collections"] = ImmutableAndFrozenDemo.RunImmutableAsync,
        ["frozen-collections"] = ImmutableAndFrozenDemo.RunFrozenAsync,
        ["concurrent-dictionary-getoradd"] = ConcurrentCollectionsDemo.RunGetOrAddAsync,
        ["concurrent-dictionary-addorupdate"] = ConcurrentCollectionsDemo.RunAddOrUpdateAsync,
        ["concurrent-queue-producer-consumer"] = ConcurrentCollectionsDemo.RunConcurrentQueueAsync,
        ["blocking-collection"] = ConcurrentCollectionsDemo.RunBlockingCollectionAsync,
        ["collection-expressions"] = CollectionExpressionsDemo.RunExpressionsAsync,
        ["params-collections"] = CollectionExpressionsDemo.RunParamsCollectionsAsync,
        ["collection-builder"] = CollectionExpressionsDemo.RunCollectionBuilderAsync,
        ["yield-deferred-execution"] = CustomCollectionDemo.RunYieldDeferredExecutionAsync,
        ["custom-enumerable"] = CustomCollectionDemo.RunCustomEnumerableAsync,
        ["count-vs-count"] = CustomCollectionDemo.RunCountVsCountAsync,
        ["lookup-performance"] = LookupPerformanceDemo.RunLookupPerformanceAsync,
        ["presize-vs-grow"] = LookupPerformanceDemo.RunPresizeVsGrowAsync,
    };

    private static async Task Main(string[] args)
    {
        if (args is ["--list"])
        {
            foreach (var name in Demos.Keys)
            {
                Console.WriteLine(name);
            }

            return;
        }

        var namesToRun = args.Length == 0 ? [.. Demos.Keys] : args;

        foreach (var name in namesToRun)
        {
            if (!Demos.TryGetValue(name, out var run))
            {
                Console.Error.WriteLine($"Unknown demo '{name}'. Try --list.");
                Environment.ExitCode = 1;
                continue;
            }

            Console.WriteLine($"===== {name} =====");
            await run();
            Console.WriteLine();
        }
    }
}
