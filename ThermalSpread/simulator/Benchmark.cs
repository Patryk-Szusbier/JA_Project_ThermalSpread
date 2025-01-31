using static ThermalSpread.simulator.Simulation;
using System.ComponentModel;
using ThermalSpread.simulator;
using System.Diagnostics;

public record BenchmarkResult(double ElapsedMilliseconds);

public class Benchmark : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    readonly TemperaturesMatrix matrix;
    readonly Dictionary<string, Func<TemperaturesMatrix, Simulation>> simulations;
    readonly Dictionary<string, Dictionary<int, BenchmarkResult>> results = new();

    public Benchmark(TemperaturesMatrix matrix, Dictionary<string, Func<TemperaturesMatrix, Simulation>> simulations)
    {
        this.matrix = matrix;
        this.simulations = simulations;
    }

    public event Action<(string simulationId, Dictionary<int, BenchmarkResult> result)>? onSingleSimulationBenchMarkFinished;
    public event Action<Dictionary<string, Dictionary<int, BenchmarkResult>>>? onAllSimulationsBenchMarkFinished;
    public event Action<(string simulationId, SimulationStepResult simulationResult)>? onSimulationStepFinished;
    public event Action<(string simulationId, int nrOfThreads, BenchmarkResult result)>? onSimulationThreadFinished;

    public bool IsRunning { get => benchmarkTask != null; }
    private Task? _benchmarkTask = null;
    private Task? benchmarkTask
    {
        get => _benchmarkTask;
        set
        {
            _benchmarkTask = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }
    }

    CancellationTokenSource? cancelTokenTransmitter;
    public async Task Cancel()
    {
        if (!IsRunning)
        {
            throw new Exception("Cannot cancel not running benchmark");
        }

        await Task.Run(() => {
            cancelTokenTransmitter?.Cancel();

            while (benchmarkTask != null) ;
        });
    }

    public void Run(int maxNrOfThreads)
    {
        cancelTokenTransmitter = new CancellationTokenSource();
        var cancelTokenReceiver = cancelTokenTransmitter.Token;

        var benchmarkThread = new Thread(() =>
        {
            foreach (var (id, simulationConstructor) in simulations)
            {
                var records = new Dictionary<int, BenchmarkResult>();

                if (cancelTokenReceiver.IsCancellationRequested)
                {
                    return;
                }

                var threadsSimulation = simulationConstructor(matrix);
                var threads = new List<Thread>();

                for (var nrOfThreads = 0; nrOfThreads < maxNrOfThreads; nrOfThreads++)
                {
                    if (cancelTokenReceiver.IsCancellationRequested)
                    {
                        return;
                    }

                    int threadIndex = nrOfThreads; // Unikamy problemów z zakresami zmiennych w lambdach
                    var thread = new Thread(() =>
                    {
                        threadsSimulation.Run(
                            simulationStarted: _ => { },
                            stepFinished: (result) => {
                                var elapsedMs = (result.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
                                lock (records)
                                {
                                    records[threadIndex + 1] = new BenchmarkResult(elapsedMs);
                                }
                                onSimulationStepFinished?.Invoke((id, result));
                            }, _ => { }, 1, minStepMs: 0);
                    });

                    threads.Add(thread);
                    thread.Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                results[id] = records;
                onSingleSimulationBenchMarkFinished?.Invoke((id, records));
            }

            onAllSimulationsBenchMarkFinished?.Invoke(results);
        });

        benchmarkThread.Start();
    }


    public string GetAsCsvString()
    {
        var groupedData = results.SelectMany(kv1 => kv1.Value.Select(kv2 => new { StringKey = kv1.Key, IntKey = kv2.Key, Result = kv2.Value }))
                     .GroupBy(item => item.StringKey)
                     .ToDictionary(group => group.Key,
                                   group => group.GroupBy(item => item.IntKey)
                                                  .ToDictionary(innerGroup => innerGroup.Key,
                                                                innerGroup => innerGroup.Select(item => item.Result).ToList()));

        // Generowanie CSV z milisekundami
        var csvContent = "ID,NrOfThreads,ElapsedMilliseconds\n";

        foreach (var stringKey in groupedData.Keys)
        {
            foreach (var intKey in groupedData[stringKey].Keys)
            {
                foreach (var result in groupedData[stringKey][intKey])
                {
                    // Zapisujemy czas w milisekundach, ograniczając do 3 miejsc po przecinku
                    csvContent += $"{stringKey},{intKey},{result.ElapsedMilliseconds:F3}\n";
                }

                csvContent += "\n";
            }

            csvContent += "\n";
        }

        return csvContent;
    }
}
