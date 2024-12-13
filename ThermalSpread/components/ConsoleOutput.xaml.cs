using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;

namespace ThermalSpread.components;

/// <summary>
/// Logika interakcji dla klasy ConsoleOutput.xaml
/// </summary>
public partial class ConsoleOutput : UserControl
{
    public enum MessageLevel
    {
        Success,
        Info,
        Error
    }

    static readonly Dictionary<MessageLevel, SolidColorBrush> ColorsMap = new Dictionary<MessageLevel, SolidColorBrush>
    {
            { MessageLevel.Success, new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#7FC955"))},
            { MessageLevel.Info, new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#FFFFFF")) },
            { MessageLevel.Error, new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#F23913")) }
        };

    public ConsoleOutput()
    {
        InitializeComponent();
    }

    public void WriteLine(MessageLevel messageLevel, string message)
    {
        var paragraph = new Paragraph();
        var run = new Run("> " + message);
        run.Foreground = ColorsMap[messageLevel];

        paragraph.Inlines.Add(run);
        textBox.Document.Blocks.Add(paragraph);

        scroller.ScrollToBottom();
    }
}
