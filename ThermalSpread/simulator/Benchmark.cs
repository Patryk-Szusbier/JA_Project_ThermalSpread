using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ThermalSpread.simulator.Simulation;

namespace ThermalSpread.simulator;

public record BenchmarkResult(long ElapsedTicks);

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

    public Task Run(int maxNrOfThreads)
    {
        cancelTokenTransmitter = new CancellationTokenSource();
        var cancelTokenReceiver = cancelTokenTransmitter.Token;

        benchmarkTask = Task.Run(async () => {
            foreach (var (id, simulationConstructor) in simulations)
            {
                var records = new Dictionary<int, BenchmarkResult>();

                if (cancelTokenReceiver.IsCancellationRequested)
                {
                    benchmarkTask = null;
                    return;
                }

                for (var nrOfThreads = 1; nrOfThreads < maxNrOfThreads; nrOfThreads += 1)
                {
                    if (cancelTokenReceiver.IsCancellationRequested)
                    {
                        benchmarkTask = null;
                        return;
                    }

                    var threadsSimulation = simulationConstructor(matrix);

                    await threadsSimulation.Run(
                        simulationStarted: _ => { },
                        stepFinished: (result) => {
                            if (results.ContainsKey(id))
                            {
                                records[nrOfThreads] = new BenchmarkResult(records[nrOfThreads].ElapsedTicks + result.ElapsedTicks);
                            }
                            else
                            {
                                records[nrOfThreads] = new BenchmarkResult(result.ElapsedTicks);
                            }

                            onSimulationStepFinished?.Invoke((id, result));
                        }, _ => { }, nrOfThreads, minStepMs: 0);

                    onSimulationThreadFinished?.Invoke((id, nrOfThreads, records[nrOfThreads]));
                }

                results[id] = records;
                onSingleSimulationBenchMarkFinished?.Invoke((id, records));
            }

            onAllSimulationsBenchMarkFinished?.Invoke(results);
            benchmarkTask = null;
        });

        return benchmarkTask;
    }

    public string GetAsCsvString()
    {
        var groupedData = results.SelectMany(kv1 => kv1.Value.Select(kv2 => new { StringKey = kv1.Key, IntKey = kv2.Key, Result = kv2.Value }))
                     .GroupBy(item => item.StringKey)
                     .ToDictionary(group => group.Key,
                                   group => group.GroupBy(item => item.IntKey)
                                                  .ToDictionary(innerGroup => innerGroup.Key,
                                                                innerGroup => innerGroup.Select(item => item.Result).ToList()));

        // Generate CSV
        var csvContent = "ID,NrOfThreads,ElapsedTicks\n";

        foreach (var stringKey in groupedData.Keys)
        {
            foreach (var intKey in groupedData[stringKey].Keys)
            {
                foreach (var result in groupedData[stringKey][intKey])
                {
                    csvContent += $"{stringKey},{intKey},{result.ElapsedTicks}\n";
                }

                csvContent += "\n";
            }

            csvContent += "\n";
        }

        return csvContent;
    }
}
