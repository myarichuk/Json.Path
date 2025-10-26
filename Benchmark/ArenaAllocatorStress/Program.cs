using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonPath.Parser.Allocators;

namespace ArenaAllocatorStress;

internal static class Program
{
    private static readonly string Separator = new('-', 72);

    public static int Main(string[] args)
    {
        var options = StressOptions.Parse(args);

        Console.WriteLine("Arena Allocator Stress Runner");
        Console.WriteLine($"Runs: {options.Runs}, Workers: {options.WorkerCount}, Allocations/Worker: {options.AllocationsPerWorker}");
        Console.WriteLine($"Finalizer Waves: {options.FinalizerWaves}, Allocations/Wave: {options.FinalizerAllocationsPerWave}");
        Console.WriteLine(Separator);

        var scenarios = new[]
        {
            new Scenario(
                "Sequential allocation",
                () => StressScenarios.SequentialAllocation(options.WorkerCount, options.AllocationsPerWorker)),
            new Scenario(
                "Dispose race",
                () => StressScenarios.DisposeRace(Math.Max(1, options.AllocationsPerWorker * Math.Max(1, options.WorkerCount / 2)))),
            new Scenario(
                "Finalizer pressure",
                () => StressScenarios.FinalizerPressure(options.FinalizerWaves, options.FinalizerAllocationsPerWave))
        };

        foreach (var scenario in scenarios)
        {
            var latencies = new List<TimeSpan>(options.Runs);
            ScenarioResult? lastResult = null;

            for (var run = 0; run < options.Runs; run++)
            {
                var sw = Stopwatch.StartNew();
                lastResult = scenario.Execute();
                sw.Stop();
                latencies.Add(sw.Elapsed);
            }

            PrintStats(scenario.Name, latencies, lastResult);
            Console.WriteLine(Separator);
        }

        return 0;
    }

    private static void PrintStats(string scenarioName, IReadOnlyList<TimeSpan> latencies, ScenarioResult? lastResult)
    {
        var ordered = latencies.OrderBy(t => t).ToArray();
        var average = TimeSpan.FromMilliseconds(latencies.Average(ts => ts.TotalMilliseconds));
        var median = ordered[ordered.Length / 2];
        var p95 = Percentile(ordered, 0.95);
        var min = ordered[0];
        var max = ordered[^1];

        Console.WriteLine(scenarioName);
        Console.WriteLine($"  Runs: {latencies.Count}");
        Console.WriteLine($"  Min / Median / Max: {Format(min)} / {Format(median)} / {Format(max)}");
        Console.WriteLine($"  Mean: {Format(average)} | P95: {Format(p95)}");

        if (lastResult is { Summary: { Length: > 0 } summary })
        {
            Console.WriteLine($"  Last run summary: {summary}");
        }
    }

    private static TimeSpan Percentile(IReadOnlyList<TimeSpan> orderedLatencies, double percentile)
    {
        if (orderedLatencies.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var clamped = Math.Clamp(percentile, 0d, 1d);
        var index = (int)Math.Round((orderedLatencies.Count - 1) * clamped, MidpointRounding.AwayFromZero);
        return orderedLatencies[index];
    }

    private static string Format(TimeSpan span) => span.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture) + " ms";
}

internal readonly record struct Scenario(string Name, Func<ScenarioResult> Execute);

internal readonly record struct ScenarioResult(string Summary);

internal sealed class StressOptions
{
    public int Runs { get; set; } = 5;

    public int WorkerCount { get; set; } = Math.Max(2, Environment.ProcessorCount);

    public int AllocationsPerWorker { get; set; } = 20_000;

    public int FinalizerWaves { get; set; } = 96;

    public int FinalizerAllocationsPerWave { get; set; } = 256;

    public static StressOptions Parse(string[] args)
    {
        var options = new StressOptions();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--runs" when TryReadInt(args, ref i, out var runs):
                    options.Runs = Math.Max(1, runs);
                    break;
                case "--workers" when TryReadInt(args, ref i, out var workers):
                    options.WorkerCount = Math.Max(1, workers);
                    break;
                case "--allocations" when TryReadInt(args, ref i, out var allocations):
                    options.AllocationsPerWorker = Math.Max(1, allocations);
                    break;
                case "--waves" when TryReadInt(args, ref i, out var waves):
                    options.FinalizerWaves = Math.Max(1, waves);
                    break;
                case "--wave-allocations" when TryReadInt(args, ref i, out var waveAllocations):
                    options.FinalizerAllocationsPerWave = Math.Max(1, waveAllocations);
                    break;
            }
        }

        return options;
    }

    private static bool TryReadInt(string[] args, ref int index, out int value)
    {
        if (index + 1 < args.Length && int.TryParse(args[index + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            index++;
            return true;
        }

        value = 0;
        return false;
    }
}

internal static class StressScenarios
{
    public static ScenarioResult SequentialAllocation(int workerCount, int allocationsPerWorker)
    {
        var total = (long)workerCount * allocationsPerWorker;

        using var arena = new ArenaAllocator(512 * 1024);

        unsafe
        {
            for (var i = 0L; i < total; i++)
            {
                var size = (nuint)Random.Shared.Next(32, 8 * 1024);
                var align = Random.Shared.Next(0, 2) == 0 ? 0 : (nuint)(IntPtr.Size << Random.Shared.Next(0, 4));
                var buffer = (byte*)arena.Alloc(size, align);
                var label = workerCount > 0 ? (byte)((i % workerCount) + 1) : (byte)1;
                Touch(buffer, (int)size, label);
            }
        }

        return new ScenarioResult($"Executed {total:N0} sequential allocations.");
    }

    public static ScenarioResult DisposeRace(int allocationCount)
    {
        var arena = new ArenaAllocator(64 * 1024);
        var performed = 0;
        var disposing = 0;
        var inCriticalSection = 0;

        var worker = Task.Run(() =>
        {
            unsafe
            {
                try
                {
                    for (var i = 0; i < allocationCount; i++)
                    {
                        if (Volatile.Read(ref disposing) != 0)
                        {
                            break;
                        }

                        Interlocked.Exchange(ref inCriticalSection, 1);

                        if (Volatile.Read(ref disposing) != 0)
                        {
                            Interlocked.Exchange(ref inCriticalSection, 0);
                            break;
                        }

                        var size = (nuint)Random.Shared.Next(16, 4 * 1024);
                        var buffer = (byte*)arena.Alloc(size);
                        Touch(buffer, (int)size, 0xAA);
                        Interlocked.Exchange(ref inCriticalSection, 0);

                        Interlocked.Increment(ref performed);
                    }
                }
                catch (ObjectDisposedException)
                {
                    Interlocked.Exchange(ref inCriticalSection, 0);
                }
            }
        });

        var threshold = Math.Max(1, allocationCount / 3);
        var spinner = new SpinWait();

        while (Volatile.Read(ref performed) < threshold && !worker.IsCompleted)
        {
            spinner.SpinOnce();
        }

        Volatile.Write(ref disposing, 1);

        while (Volatile.Read(ref inCriticalSection) != 0)
        {
            spinner.SpinOnce();
        }

        arena.Dispose();
        worker.GetAwaiter().GetResult();

        return new ScenarioResult($"Performed {performed:N0} allocations before dispose completed.");
    }

    public static ScenarioResult FinalizerPressure(int waveCount, int allocationsPerWave)
    {
        var refs = new WeakReference[waveCount];

        for (var i = 0; i < waveCount; i++)
        {
            refs[i] = CreateFinalizerTarget(allocationsPerWave);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var survivors = refs.Count(static r => r.IsAlive);
        return new ScenarioResult($"Finalizer survivors after forced GC: {survivors} of {refs.Length}.");
    }

    private static unsafe WeakReference CreateFinalizerTarget(int allocations)
    {
        var arena = new ArenaAllocator(32 * 1024);

        for (var i = 0; i < allocations; i++)
        {
            var size = (nuint)Random.Shared.Next(64, 16 * 1024);
            var buffer = (byte*)arena.Alloc(size);
            Touch(buffer, (int)size, (byte)(i & 0xFF));
        }

        return new WeakReference(arena);
    }

    private static unsafe void Touch(byte* buffer, int size, byte value)
    {
        var span = new Span<byte>(buffer, size);
        span[0] = value;
        span[^1] = value;

        var stride = Math.Max(1, size / 16);
        for (var i = stride; i < size - 1; i += stride)
        {
            span[i] = (byte)(value ^ (i & 0xFF));
        }
    }
}
