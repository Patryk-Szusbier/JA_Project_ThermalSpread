using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ThermalSpread.components;

public partial class FileInput : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly SolidColorBrush CorrectColor = new SolidColorBrush(Colors.Black);
    private static readonly SolidColorBrush ErrorColor = new SolidColorBrush(Colors.Red);

    //props
    public static readonly DependencyProperty InputLabelProperty = DependencyProperty.Register(nameof(InputLabel), typeof(string), typeof(FileInput), new PropertyMetadata(null));
    public static readonly DependencyProperty FileExtensionProperty = DependencyProperty.Register(nameof(FileExtension), typeof(string), typeof(FileInput), new PropertyMetadata(null));
    public string InputLabel
    {
        get { return (string)GetValue(InputLabelProperty); }
        set { SetValue(InputLabelProperty, value); }
    }

    public string FileExtension
    {
        get { return (string)GetValue(FileExtensionProperty); }
        set { SetValue(FileExtensionProperty, value); }
    }

    public string FilePath { get; set; } = "";
    public bool IsCorrect { get; set; } = false;

    public event EventHandler<string>? OnAction;

    public FileInput()
    {
        InitializeComponent();
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e) => OnAction?.Invoke(this, FilePath);

    private void OpenExplorerButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = $"${InputLabel} |*.{FileExtension}";
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if (openFileDialog.ShowDialog() == true)
        {
            textBox.Text = openFileDialog.FileName;
        }
    }

    private void textBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        IsCorrect = File.Exists(textBox.Text);
        textBox.Foreground = IsCorrect ? CorrectColor : ErrorColor;
    }
}
