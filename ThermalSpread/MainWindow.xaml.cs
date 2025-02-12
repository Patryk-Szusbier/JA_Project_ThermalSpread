using ThermalSpread.components;
using ThermalSpread.simulator;
using ThermalSpread.utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Point = System.Drawing.Point;
using ThermalSpread.simulator.implementations;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;

namespace ThermalSpread;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private Dictionary<string, Func<TemperaturesMatrix, Simulation>> getSimulationsList()
    {
        return new Dictionary<string, Func<TemperaturesMatrix, Simulation>> {
           { "ASM vector", matrix => new VectorAsmSimulation(Matrix.Clone()) },
           { "C#", matrix => new CSharpSimulation(Matrix.Clone()) },
            { "C++", matrix => new CppSimulation(Matrix.Clone()) },
        };
    }

    private static TemperaturesMatrix getDefaultTemperaturesMatrix(IEnumerable<(Point point, byte value)>? initialTemperatures = null, IEnumerable<(Point point, byte value)>? constantTemperatures = null, int width = 64, int height = 64)
    {
        var newInitialTemperatures = initialTemperatures ?? new List<(Point point, byte value)>();
        var newConstantTemperatures = constantTemperatures ?? new List<(Point point, byte value)>();

        return new TemperaturesMatrix(new Config(width, height, newInitialTemperatures, newConstantTemperatures, 5));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public InteractivityConfig InteractivityConfig { get; set; } = InteractivityPanel.DefaultInteractivityConfig;

    public bool IsAnySimulationRunning { get; set; } = false;

    public SimulationTarget SimulationTarget { get; set; } = SimulationTarget.CPP;

    public Gradient Gradient { get; } = new Gradient((Color)ColorConverter.ConvertFromString("#0000FF"), (Color)ColorConverter.ConvertFromString("#FF0000"));
    public bool UpdateUI { get; set; } = true;
    public int MinStepMs { get; set; } = 0;
    public int NrOfThreads { get; set; } = Environment.ProcessorCount;

    public TemperaturesMatrix Matrix { get; set; }
    public Simulation? Simulation { get; set; } = null;
    public Benchmark? Benchmark { get; set; } = null;

    public MainWindow()
    {
        Matrix = getDefaultTemperaturesMatrix();

        InitializeComponent();
        console.WriteLine(ConsoleOutput.MessageLevel.Info, "Welcome to simple Thermal simulator");
        console.WriteLine(ConsoleOutput.MessageLevel.Info, "Go to interactivity section, select preset and start simulation");
        // Sprawdzenie, czy procesor obsługuje AVX-512
        if (!IsAVX512Supported())
        {
            console.WriteLine(ConsoleOutput.MessageLevel.Error, "AVX-512 is not supported on this processor. Assembler DLL is been disable");
        }
        else
        {
            console.WriteLine(ConsoleOutput.MessageLevel.Info, "AVX-512 is supported.");
        }

        
    }

    private bool IsAVX512Supported()
    {
        try
        {
            // Pobieranie informacji o procesorze
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                // Sprawdzanie, czy wśród cech procesora znajduje się AVX-512
                string processorId = queryObj["ProcessorId"]?.ToString();
                string caption = queryObj["Caption"]?.ToString();
                string instructionSet = queryObj["InstructionSet"]?.ToString();

                if (!string.IsNullOrEmpty(instructionSet) && instructionSet.Contains("AVX512"))
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
        }

        return false;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        temperaturesCanvas.RenderMatrix(Matrix);
    }

    //Simulation Panel handling
    private void SimulationPanel_OnStart(object sender, EventArgs e)
    {
        if (Simulation?.IsRunning == true)
        {
            return;
        }

        console.WriteLine(ConsoleOutput.MessageLevel.Info, $"Starting simulation, NrOfThreads={NrOfThreads}");

        Simulation =
            SimulationTarget == SimulationTarget.ASM_VECTOR ? new VectorAsmSimulation(Matrix) : 
            SimulationTarget == SimulationTarget.CPP ? new CppSimulation(Matrix) :
            SimulationTarget == SimulationTarget.C_SHARP ? new CSharpSimulation(Matrix): null;

        Simulation?.Run(
            simulationStarted: result => {
                IsAnySimulationRunning = true;
                Dispatcher.Invoke(new Action(() => {
                    console.WriteLine(ConsoleOutput.MessageLevel.Success, $"Simulation: Started, NrOfThreads={result.NrOfThreads}, Chunks={result.ThreadChunks.Select(entry => $"<{entry.from}; {entry.to}>").Aggregate("", (acc, x) => acc + x + ", ")}");
                }));
            },
            stepFinished: result => {
                Dispatcher.Invoke(new Action(() => {
                    if (UpdateUI)
                    {
                        temperaturesCanvas.RenderMatrix(result.Matrix);
                    }

                    var elapsedMs = (result.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
                    console.WriteLine(ConsoleOutput.MessageLevel.Info, $"Simulation: Step finished, Step={result.Step}, ElapsedMilliseconds={elapsedMs:F2}, Finished={result.IsEdgeConditionMet}");
                }));
            },
        simulationFinished: result => {
            IsAnySimulationRunning = false;
            Dispatcher.Invoke(new Action(() => {
                if (UpdateUI)
                {
                    temperaturesCanvas.RenderMatrix(result.Matrix);
                }

                var totalElapsedMs = (result.TotalElapsedTicks / (double)Stopwatch.Frequency) * 1000;
                console.WriteLine(ConsoleOutput.MessageLevel.Success, $"Simulation: Finished, NrOfSteps={result.NrOfSteps}, Milliseconds={totalElapsedMs:F2}");
            }));
        },
        NrOfThreads, MinStepMs);
    }

    private async void SimulationPanel_OnStop(object sender, System.EventArgs e)
    {
        if (Simulation?.IsRunning == true)
        {
            await Simulation!.Cancel();
            IsAnySimulationRunning = false;
            console.WriteLine(ConsoleOutput.MessageLevel.Error, $"Simulation stopped");
        }
        else if (Benchmark?.IsRunning == true)
        {
            await Benchmark!.Cancel();
            IsAnySimulationRunning = false;
            console.WriteLine(ConsoleOutput.MessageLevel.Error, $"Benchmark stopped");
        }
    }

    private void SimulationPanel_OnStartBenchmark(object sender, System.EventArgs e)
    {
#if DEBUG
        console.WriteLine(ConsoleOutput.MessageLevel.Error, "Attempting to run benchmark in DEBUG mode, aborting");
        return;
#else
        console.WriteLine(ConsoleOutput.MessageLevel.Info, "Starting benchmark for ASM and CPP...");
        IsAnySimulationRunning = true;
        Benchmark = new Benchmark(Matrix, getSimulationsList());
        Benchmark.onSimulationStepFinished += param => {
            Dispatcher.Invoke(new Action(() => {
                if (UpdateUI)
                {
                    temperaturesCanvas.RenderMatrix(param.simulationResult.Matrix);
                }

                var elapsedMs = (param.simulationResult.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
                console.WriteLine(ConsoleOutput.MessageLevel.Success, $"Benchmark step for {param.simulationId} finished | NrOfThreads={param.simulationResult.NrOfThreads} | ElapsedMilliseconds={elapsedMs:F2}");
            }));
        };
        Benchmark.onSingleSimulationBenchMarkFinished += param => {
            Dispatcher.Invoke(new Action(() => {
                console.WriteLine(ConsoleOutput.MessageLevel.Success, $"Benchmark for {param.simulationId} finished");
            }));
        };
        Benchmark.onAllSimulationsBenchMarkFinished += obj => {
            IsAnySimulationRunning = false;
            Dispatcher.Invoke(new Action(() => {
                playNotificationSound();

                var openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                console.WriteLine(ConsoleOutput.MessageLevel.Success, "Benchmark finished");
                if (openFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(openFileDialog.FileName, Benchmark!.GetAsCsvString());
                }


                Benchmark = null;
            }));
        };

        Benchmark.Run(64);
#endif
    }

    private void SimulationPanel_OnReset(object sender, System.EventArgs e)
    {
        Matrix = getDefaultTemperaturesMatrix();

        temperaturesCanvas.RenderMatrix(Matrix);
    }

    //Configuration Panel handling
    private void ConfigurationPanel_ConfigFileLoadRequest(string path)
    {
        try
        {
            var asText = File.ReadAllText(path);

            var config = JsonSerializer.Deserialize<Config>(asText);
            if (config == null)
            {
                console.WriteLine(ConsoleOutput.MessageLevel.Error, $"Provided invalid file at {path}");
                return;
            }
            else if (!config.isCorrect())
            {
                console.WriteLine(ConsoleOutput.MessageLevel.Error, $"Provided malformed file at {path}");
                return;
            }

            Matrix = new TemperaturesMatrix(config);
            temperaturesCanvas.RenderMatrix(Matrix);
            console.WriteLine(ConsoleOutput.MessageLevel.Success, $"Loaded configuration file from {path}");
        }
        catch (Exception e)
        {
            console.WriteLine(ConsoleOutput.MessageLevel.Error, e.ToString());
        }
    }

    private async void ConfigurationPanel_ConfigFileWriteRequest(string path)
    {
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(Matrix.Config));
            console.WriteLine(ConsoleOutput.MessageLevel.Success, $"Saved configuration file to {path}");
        }
        catch (Exception e)
        {
            console.WriteLine(ConsoleOutput.MessageLevel.Error, e.ToString());
        }
    }

    //Interactivity Panel handling
    private void InteractivityPanel_OnConfigChanged(InteractivityConfig newConfig)
    {
        InteractivityConfig = newConfig;
    }

    private void InteractivityPanel_OnPresetSelected(BytePainter bytePainter)
    {
        IEnumerable<(Point point, byte value)> initialTemperatures;
        IEnumerable<(Point point, byte value)> constantTemperatures;
        if (InteractivityConfig.TemperatureType == InteractivityConfig.TemperatureTypeEnum.Constant)
        {
            initialTemperatures = new List<(Point point, byte value)> { };
            constantTemperatures = bytePainter.GetChanges().ToList();
        }
        else
        {
            initialTemperatures = bytePainter.GetChanges().ToList();
            constantTemperatures = new List<(Point point, byte value)> { };
        }

        Matrix = getDefaultTemperaturesMatrix(initialTemperatures, constantTemperatures);
        temperaturesCanvas.RenderMatrix(Matrix);
    }

    //Temperatures Canvas handling
    private async void TemperaturesCanvas_OnDrawn(BytePainter bytePainter)
    {
        var changes = bytePainter.GetChanges();

        Matrix.setPoints(changes);
        Matrix.Config = InteractivityConfig.TemperatureType == InteractivityConfig.TemperatureTypeEnum.Constant ? new Config(
                Matrix.Config.Width,
                Matrix.Config.Height,
                Matrix.Config.InitialTemperatures,
                 Matrix.Config.ConstantTemperatures.Concat(changes).ToList(),
                Matrix.Config.AlfaCoeff
            ) : Matrix.Config;

        temperaturesCanvas.RenderMatrix(Matrix);
    }

    private async void TemperaturesCanvas_OnSizeChanged(int newWidth, int newHeight)
    {
        if (Simulation != null && Simulation.IsRunning)
        {
            await Simulation?.Cancel();
            IsAnySimulationRunning = false;
        }

        Matrix = getDefaultTemperaturesMatrix(null, null, newWidth, newHeight);

        temperaturesCanvas.RenderMatrix(Matrix);
    }

    private void playNotificationSound()
    {
        bool found = false;
        try
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes\Apps\.Default\Notification.Default\.Current");
            if (key != null)
            {
                var o = key.GetValue(null); // pass null to get (Default)
                if (o != null)
                {
                    SoundPlayer theSound = new((string)o);
                    theSound.Play();
                    found = true;
                }
            }
        }
        catch { }
        if (!found)
            SystemSounds.Beep.Play();
    }
}