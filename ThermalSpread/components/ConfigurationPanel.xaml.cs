using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ThermalSpread.components;

public partial class ConfigurationPanel : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ConfigurationPanel()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty AlfaCoeffProperty = DependencyProperty.Register(nameof(AlfaCoeff), typeof(double), typeof(ConfigurationPanel), new PropertyMetadata(0.0));

    public double AlfaCoeff
    {
        get { return (double)GetValue(AlfaCoeffProperty); }
        set
        {
            SetValue(AlfaCoeffProperty, value);
        }
    }

    public event Action<string>? ConfigFileLoadRequest;
    public event Action<string>? ConfigFileWriteRequest;

    private void ConfigFileLoad_OnAction(object sender, string path)
    {
        ConfigFileLoadRequest?.Invoke(path);
    }

    private void ConfigFileWrite_OnAction(object sender, string path)
    {
        ConfigFileWriteRequest?.Invoke(path);
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is Slider slider && e.NewValue == 0)
        {
            slider.Value = e.OldValue;
        }
    }
}