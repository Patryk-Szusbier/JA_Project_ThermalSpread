using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Temperature = System.Byte;

namespace ThermalSpread.simulator;

public abstract class Simulation : INotifyPropertyChanged
{
    private record MagicMultplicationCoefficient(ushort Shift, int Mul);

    private static readonly Dictionary<int, MagicMultplicationCoefficient> MagicMultplicationCoefficients = new() {
        {-20, new(1639, 15)},
        {-19, new(1725, 15)},
        {-18, new(1821, 15)},
        {-17, new(241, 12)},
        {-16, new(1, 4)},
        {-15, new(1093, 14)},
        {-14, new(1171, 14)},
        {-13, new(1261, 14)},
        {-12, new(683, 13)},
        {-11, new(745, 13)},
        {-10, new(1639, 14)},
        {-9, new(1821, 14)},
        {-8, new(1, 3)},
        {-7, new(1171, 13)},
        {-6, new(683, 12)},
        {-5, new(1639, 13)},
        {-4, new(1, 2)},
        {-3, new(683, 11)},
        {-2, new(1, 1)},
        {-1, new(1, 0)},
        {1, new(1, 0)},
        {2, new(1, 1)},
        {3, new(683, 11)},
        {4, new(1, 2)},
        {5, new(1639, 13)},
        {6, new(683, 12)},
        {7, new(1171, 13)},
        {8, new(1, 3)},
        {9, new(1821, 14)},
        {10, new(1639, 14)},
        {11, new(745, 13)},
        {12, new(683, 13)},
        {13, new(1261, 14)},
        {14, new(1171, 14)},
        {15, new(1093, 14)},
        {16, new(1, 4)},
        {17, new(241, 12)},
        {18, new(1821, 15)},
        {19, new(1725, 15)},
        {20, new(1639, 15)}
 };

    public event PropertyChangedEventHandler? PropertyChanged;

    public record SimulationResult(TemperaturesMatrix Matrix, long TotalElapsedTicks, int NrOfSteps);
    public record SimulationStartInfo(TemperaturesMatrix Matrix, int NrOfThreads, (int from, int to)[] ThreadChunks);
    public record SimulationStepResult(TemperaturesMatrix Matrix, long ElapsedTicks, int NrOfThreads, bool IsEdgeConditionMet, int Step);

    (int from, int to)[] _chunks;
    int runsCounter;

    readonly TemperaturesMatrix matrixA;
    readonly TemperaturesMatrix matrixB;

    static (int from, int to)[] divideMatrix(int arrayWidth, int nrOfThreads)
    {
        var chunkSize = arrayWidth / nrOfThreads;
        var remainder = arrayWidth % nrOfThreads;

        return Enumerable.Range(0, nrOfThreads)
            .Select(i => {
                int from = i * chunkSize;
                int to = (i + 1) * chunkSize - 1;

                if (i == 0)
                {
                    from += 1;
                }

                if (i == nrOfThreads - 1)
                {
                    //pomiń ostatni pixel
                    to += remainder - 1;
                }

                return (from, to);
            })
            .ToArray();
    }

    private (TemperaturesMatrix readMatrix, TemperaturesMatrix writeMatrix) getMatrices()
    {
        var isRunEven = runsCounter % 2 == 0;

        return (
            isRunEven ? matrixA : matrixB,
            !isRunEven ? matrixA : matrixB
        );
    }

    volatile bool areAllEdgeConditionsMet = true;
    private SimulationStepResult runStep(int nrOfThreads, long minStepMs)
    {
        var (readMatrix, writeMatrix) = getMatrices();
        var config = readMatrix.Config;

        areAllEdgeConditionsMet = true;

        var actualHeight = config.Height - 2;

        var (mul, shift) = MagicMultplicationCoefficients[matrixA.Config.AlfaCoeff];

        var readByteArray = readMatrix.getByteArray();
        var writeByteArray = writeMatrix.getByteArray();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var threads = new List<Thread>();

        for (int i = 0; i < nrOfThreads; i++)
        {
            int index = i; // Unikamy problemów z zakresami zmiennych w lambdach
            var thread = new Thread(() => {
                var result = runSingleStep(
                    readByteArray,
                    writeByteArray,
                    config.Width,
                    actualHeight,
                    _chunks[index].from, _chunks[index].to, mul, shift);

                if (!result && areAllEdgeConditionsMet)
                {
                    areAllEdgeConditionsMet = false;
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();

        if (minStepMs > stopwatch.ElapsedMilliseconds)
        {
            Thread.Sleep((int)(minStepMs - stopwatch.ElapsedMilliseconds));
        }

        foreach (var (point, temperature) in writeMatrix.Config.ConstantTemperatures)
        {
            writeMatrix.setPoint(temperature, point);
        }

        return new SimulationStepResult(writeMatrix, stopwatch.ElapsedTicks, nrOfThreads, areAllEdgeConditionsMet, runsCounter);
    }


    public Simulation(TemperaturesMatrix temperaturesMatrix)
    {
        matrixA = temperaturesMatrix;
        matrixB = temperaturesMatrix.Clone();
    }

    CancellationTokenSource? cancelTokenTransmitter;

    private Task? _simulationTask = null;
    private Task? simulationTask
    {
        get => _simulationTask;
        set
        {
            _simulationTask = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }
    }
    public bool IsRunning { get => simulationTask != null; }

    private readonly Channel<bool> channel = Channel.CreateUnbounded<bool>();

    public async Task Cancel()
    {
        if (!IsRunning)
        {
            throw new Exception("Cannot cancel not running simulation");
        }

        await Task.Run(() => {
            cancelTokenTransmitter?.Cancel(); // Anulowanie symulacji

            // Czekamy na zakończenie zadania (wątku)
            while (simulationTask != null) ;
        });
    }

    public async Task Pause()
    {
        if (!IsRunning)
        {
            throw new Exception("Cannot pause not running simulation");
        }

        await Task.Run(() => {
            while (!channel.Writer.TryWrite(true)) ;
            while (channel.Reader.Count == 0) ;
            var readMessage = false;
            do
            {
                channel.Reader.TryRead(out readMessage);
            } while (!readMessage);
        });
    }

    public async Task Resume()
    {
        if (!IsRunning)
        {
            throw new Exception("Cannot resume not running simulation");
        }

        await Task.Run(() => {
            while (!channel.Writer.TryWrite(false)) ;
            while (channel.Reader.Count == 0) ;
            var readMessage = false;
            do
            {
                channel.Reader.TryRead(out readMessage);
            } while (!readMessage);
        });
    }

    protected abstract bool runSingleStep(Temperature[] readData, Temperature[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift);

    public void Run(Action<SimulationStartInfo> simulationStarted, Action<SimulationStepResult> stepFinished, Action<SimulationResult> simulationFinished, int nrOfThreads, long minStepMs)
    {
        _chunks = divideMatrix(matrixA.Config.Width, nrOfThreads);
        simulationStarted(new SimulationStartInfo(matrixA, nrOfThreads, _chunks));
        runsCounter = 0;

        cancelTokenTransmitter = new CancellationTokenSource(); // Utwórz token anulowania
        var cancelTokenReceiver = cancelTokenTransmitter.Token;

        long totalElapsedTicks = 0;

        var simulationThread = new Thread(() =>
        {
            SimulationStepResult step;

            do
            {
                // Sprawdzanie, czy żądano anulowania
                if (cancelTokenReceiver.IsCancellationRequested)
                {
                    return; // Zakończ wątek, jeśli anulowano
                }

                step = runStep(nrOfThreads, minStepMs); // Wykonaj krok symulacji
                runsCounter += 1;

                // Sprawdzanie stanu pauzy
                if (channel.Reader.Count > 0)
                {
                    if (channel.Reader.TryRead(out bool message) && message)
                    {
                        channel.Writer.TryWrite(true);

                        do
                        {
                            channel.Reader.TryRead(out message);
                        } while (message);

                        channel.Writer.TryWrite(true);
                    }
                }

                stepFinished(step); // Zakończenie kroku symulacji
                totalElapsedTicks += step.ElapsedTicks;

            } while (!step.IsEdgeConditionMet && !cancelTokenReceiver.IsCancellationRequested); // Zakończenie na podstawie stanu brzegowego lub anulowania

            // Po zakończeniu symulacji
            simulationFinished(new SimulationResult(step.Matrix, totalElapsedTicks, step.Step));
        });

        simulationThread.Start(); // Rozpocznij wątek symulacji
    }

}