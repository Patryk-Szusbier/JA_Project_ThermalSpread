using ThermalSpread.simulator;
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
using ThermalSpread.components;
using ThermalSpread.utils;

namespace ThermalSpread;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IMainWindow
{
    private static TemperaturesMatrix getDefaultTemperaturesMatrix(IEnumerable<(Point point, byte value)>? initialTemperatures = null, IEnumerable<(Point point, byte value)>? constantTemperatures = null, int width = 64, int height = 64)
    {
        var newInitialTemperatures = initialTemperatures ?? new List<(Point point, byte value)>();
        var newConstantTemperatures = constantTemperatures ?? new List<(Point point, byte value)>();

        return new TemperaturesMatrix(new Config(width, height, newInitialTemperatures, newConstantTemperatures, 5));
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsAnySimulationRunning { get; set; } = false;

    public SimulationTarget SimulationTarget { get; set; } = SimulationTarget.ASM_VECTOR;

    public Gradient Gradient { get; } = new Gradient((Color)ColorConverter.ConvertFromString("#0000FF"), (Color)ColorConverter.ConvertFromString("#FF0000"));
    public bool UpdateUI { get; set; } = true;
    public int MinStepMs { get; set; } = 0;
    public int NrOfThreads { get; set; } = 1;

    public TemperaturesMatrix Matrix { get; set; }
    public Simulation? Simulation { get; set; } = null;
    public Benchmark? Benchmark { get; set; } = null;

    public MainWindow()
    {
        Matrix = getDefaultTemperaturesMatrix();

        InitializeComponent();

    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        temperaturesCanvas.RenderMatrix(Matrix);
    }
    private void SimulationPanel_OnStart(object sender, EventArgs e)
    {
        if (Simulation?.IsRunning == true)
        {
            return;
        }
    }
    private async void SimulationPanel_OnStop(object sender, System.EventArgs e)
    {

    }
    private void SimulationPanel_OnStartBenchmark(object sender, System.EventArgs e)
    {

    }
    private void SimulationPanel_OnReset(object sender, System.EventArgs e)
    {

    }
}