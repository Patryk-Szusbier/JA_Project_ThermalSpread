using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ThermalSpread.components;

public enum SimulationTarget
{
    CPP,
    C_SHARP,
    ASM_VECTOR
}

public partial class SimulationPanel : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;


    public SimulationPanel()
    {
        InitializeComponent();

    }

    public static readonly DependencyProperty SimulationTargetProperty = DependencyProperty.Register(nameof(SimulationTarget), typeof(SimulationTarget), typeof(SimulationPanel), new PropertyMetadata(null));
    public static readonly DependencyProperty IsSimulationRunningProperty = DependencyProperty.Register(nameof(IsSimulationRunning), typeof(bool), typeof(SimulationPanel), new PropertyMetadata(false));
    public static readonly DependencyProperty UpdateUIProperty = DependencyProperty.Register(nameof(UpdateUI), typeof(bool), typeof(SimulationPanel), new PropertyMetadata(false));
    public static readonly DependencyProperty MinStepMsProperty = DependencyProperty.Register(nameof(MinStepMs), typeof(int), typeof(SimulationPanel), new PropertyMetadata(0));
    public static readonly DependencyProperty NrOfThreadsProperty = DependencyProperty.Register(nameof(NrOfThreads), typeof(int), typeof(SimulationPanel), new PropertyMetadata(0));

    public SimulationTarget SimulationTarget
    {
        get { return (SimulationTarget)GetValue(SimulationTargetProperty); }
        set { SetValue(SimulationTargetProperty, value); }
    }

    public bool IsSimulationRunning
    {
        get { return (bool)GetValue(IsSimulationRunningProperty); }
        set { SetValue(IsSimulationRunningProperty, value); }
    }

    public bool UpdateUI
    {
        get { return (bool)GetValue(UpdateUIProperty); }
        set { SetValue(UpdateUIProperty, value); }
    }

    public int MinStepMs
    {
        get { return (int)GetValue(MinStepMsProperty); }
        set { SetValue(MinStepMsProperty, value); }
    }

    public int NrOfThreads
    {
        get { return (int)GetValue(NrOfThreadsProperty); }
        set { SetValue(NrOfThreadsProperty, value); }
    }

    public event EventHandler? OnStart;
    public event EventHandler? OnStop;

    public event EventHandler? OnStartBenchmark;

    public event EventHandler? OnReset;
    private void StartSimulation_Click(object sender, RoutedEventArgs e)
    {
        OnStart?.Invoke(sender, e);
    }

    private void EndSimulation_Click(object sender, RoutedEventArgs e)
    {
        OnStop?.Invoke(sender, e);
    }

    private void StartBenchmark_Click(object sender, RoutedEventArgs e)
    {
        OnStartBenchmark?.Invoke(sender, e);
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        OnReset?.Invoke(sender, e);
    }
}