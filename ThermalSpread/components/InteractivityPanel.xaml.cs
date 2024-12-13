using ThermalSpread.simulator;
using ThermalSpread.utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Point = System.Drawing.Point;

namespace ThermalSpread.components;

/// <summary>
/// Logika interakcji dla klasy InteractivityPanel.xaml
/// </summary>
public class InteractivityConfig : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public enum ShapeEnum
    {
        Circle,
        FreeForm,
        Rectangle,
        StraightLine
    }

    public enum TemperatureTypeEnum
    {
        Initial,
        Constant
    }

    public int Thickness { get; set; }
    public byte Temperature { get; set; }
    public ShapeEnum Shape { get; set; }
    public TemperatureTypeEnum TemperatureType { get; set; }

    public InteractivityConfig(int thickness, ShapeEnum shape, TemperatureTypeEnum temperatureType, byte temperature)
    {
        Thickness = thickness;
        Shape = Shape;
        TemperatureType = TemperatureType;
        Temperature = temperature;
    }
}

public partial class InteractivityPanel : UserControl, INotifyPropertyChanged
{
    public static readonly InteractivityConfig DefaultInteractivityConfig = new(1, InteractivityConfig.ShapeEnum.FreeForm, InteractivityConfig.TemperatureTypeEnum.Initial, Constants.MaxTemperature);
    public event PropertyChangedEventHandler? PropertyChanged;

    public InteractivityConfig InteractivityConfig { get; set; } = DefaultInteractivityConfig;

    public static readonly DependencyProperty GradientProperty = DependencyProperty.Register(nameof(Gradient), typeof(Gradient), typeof(InteractivityPanel), new PropertyMetadata(null));
    public static readonly DependencyProperty WidthPxProperty = DependencyProperty.Register(nameof(WidthPx), typeof(int), typeof(InteractivityPanel), new PropertyMetadata(0));
    public static readonly DependencyProperty HeightPxProperty = DependencyProperty.Register(nameof(HeightPx), typeof(int), typeof(InteractivityPanel), new PropertyMetadata(0));
    public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(int), typeof(InteractivityPanel), new PropertyMetadata(0));

    public Gradient Gradient
    {
        get { return (Gradient)GetValue(GradientProperty); }
        set { SetValue(GradientProperty, value); }
    }

    public int WidthPx
    {
        get { return (int)GetValue(WidthPxProperty); }
        set { SetValue(WidthPxProperty, value); }
    }

    public int HeightPx
    {
        get { return (int)GetValue(HeightPxProperty); }
        set { SetValue(HeightPxProperty, value); }
    }

    public int Thickness
    {
        get { return (int)GetValue(ThicknessProperty); }
        set { SetValue(ThicknessProperty, value); }
    }

    public event Action<InteractivityConfig>? OnConfigChanged;
    public event Action<BytePainter>? OnPresetSelected;

    private static readonly string SquareInCircle = nameof(SquareInCircle);
    private static readonly string Dots = nameof(Dots);
    private static readonly string RightEdge = nameof(RightEdge);
    private static readonly string Net = nameof(Net);
    private static readonly string RandomDots = nameof(RandomDots);
    private static readonly string RandomShapes = nameof(RandomShapes);
    private static readonly string Fill = nameof(Fill);

    private void onPresetButtonClicked(object sender, System.EventArgs e)
    {
        if (sender is Button button && button.Tag is string tagAsString)
        {

            var bytePainter = new BytePainter(WidthPx, HeightPx, Gradient);

            switch (tagAsString)
            {
                case nameof(SquareInCircle):
                    {
                        var centerPoint = new Point(WidthPx / 2, HeightPx / 2);

                        bytePainter.DrawCircle(Constants.MaxTemperature, centerPoint, (int)(Math.Min(WidthPx, HeightPx) * 0.5 * 0.8));
                        bytePainter.DrawRectangle(Constants.MinTemperature, new Point(centerPoint.X - WidthPx / 4, centerPoint.Y - HeightPx / 4), new Point(centerPoint.X + WidthPx / 4, centerPoint.Y + HeightPx / 4));
                        break;
                    }
                case nameof(Dots):
                    {
                        var minSize = Math.Min(WidthPx, HeightPx);
                        var dotSize = Math.Max((int)(minSize * 0.05), 1);
                        var spacing = dotSize * 3;

                        for (var x = 0; x < WidthPx; x += spacing)
                        {
                            for (var y = 0; y < HeightPx; y += spacing)
                            {
                                var centerPoint = new Point(x, y);
                                bytePainter.DrawCircle(Constants.MaxTemperature, centerPoint, dotSize);
                            }
                        }
                        break;
                    }
                case nameof(RightEdge):
                    {
                        bytePainter.DrawLine(Constants.MaxTemperature, new Point(WidthPx, 0), new Point(WidthPx, HeightPx), Thickness);
                        break;
                    }
                case nameof(Net):
                    {
                        var minSize = Math.Min(WidthPx, HeightPx);
                        var thickness = Math.Max(minSize / 50, 1);
                        var xSpacing = WidthPx / (thickness * 4);
                        var ySpacing = HeightPx / (thickness * 4);

                        for (var x = 0; x < WidthPx; x += xSpacing)
                        {
                            var startingPoint = new Point(x, 0);
                            var endingPoint = new Point(x, HeightPx);
                            bytePainter.DrawLine(Constants.MaxTemperature, startingPoint, endingPoint, thickness);
                        }

                        for (var y = 0; y < HeightPx; y += ySpacing)
                        {
                            var startingPoint = new Point(0, y);
                            var endingPoint = new Point(WidthPx, y);
                            bytePainter.DrawLine(Constants.MaxTemperature, startingPoint, endingPoint, thickness);
                        }
                        break;
                    }
                case nameof(RandomDots):
                    {
                        var rnd = new Random();
                        for (var x = 0; x < WidthPx; x += 1)
                        {
                            for (var y = 0; y < HeightPx; y += 1)
                            {
                                if (rnd.Next() % 2 == 0)
                                {
                                    bytePainter.DrawPixel(Constants.MaxTemperature, x, y);
                                }
                            }
                        }
                        break;
                    }
                case nameof(RandomShapes):
                    {
                        var rnd = new Random();
                        var minSize = Math.Min(WidthPx, HeightPx);

                        for (var i = 0; i < 4; i += 1)
                        {
                            var position = new Point(rnd.Next() % WidthPx, rnd.Next() % HeightPx);

                            switch (rnd.Next() % 3)
                            {
                                case 0:
                                    bytePainter.DrawCircle(Constants.MaxTemperature, position, (int)(rnd.Next() % minSize * 0.8));
                                    break;
                                case 1:
                                    bytePainter.DrawLine(Constants.MaxTemperature, position, new Point(rnd.Next() % WidthPx, rnd.Next() % HeightPx), (int)(rnd.Next() % minSize * 0.8));
                                    break;
                                case 2:
                                    bytePainter.DrawRectangle(Constants.MaxTemperature, position, new Point(rnd.Next() % WidthPx, rnd.Next() % HeightPx));
                                    break;
                            }
                        }

                        break;
                    }
                case nameof(Fill):
                    {
                        bytePainter.DrawRectangle(Constants.MaxTemperature, new Point(0, 0), new Point(WidthPx - 1, HeightPx - 1));

                        break;
                    }
            }

            //removing edges
            bytePainter.DrawFrame(0, 1);

            OnPresetSelected?.Invoke(bytePainter);
        }
    }

    public InteractivityPanel()
    {
        InitializeComponent();

        var createButton = (string label, string tag) => {
            var button = new Button();
            button.Content = label;
            button.Tag = tag;
            button.Click += onPresetButtonClicked;

            return button;
        };

        examplesButtonsContainer.Children.Add(createButton("Square in circle", SquareInCircle));
        examplesButtonsContainer.Children.Add(createButton("Dots", Dots));
        examplesButtonsContainer.Children.Add(createButton("Right edge", RightEdge));
        examplesButtonsContainer.Children.Add(createButton("Net", Net));
        examplesButtonsContainer.Children.Add(createButton("Random dots", RandomDots));
        examplesButtonsContainer.Children.Add(createButton("Random shapes", RandomShapes));
        examplesButtonsContainer.Children.Add(createButton("Fill", Fill));

        InteractivityConfig.PropertyChanged += (object _, PropertyChangedEventArgs _) => OnConfigChanged?.Invoke(InteractivityConfig);
    }
}
