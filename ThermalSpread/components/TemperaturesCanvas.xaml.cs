using ThermalSpread.simulator;
using ThermalSpread.utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace ThermalSpread.components;

public partial class TemperaturesCanvas : UserControl, INotifyPropertyChanged
{
    private static readonly int MinSize = 5;

    public static readonly DependencyProperty IsInteractivityEnabledProperty = DependencyProperty.Register(nameof(IsInteractivityEnabled), typeof(bool), typeof(TemperaturesCanvas), new PropertyMetadata(false));
    public static readonly DependencyProperty GradientProperty = DependencyProperty.Register(nameof(Gradient), typeof(Gradient), typeof(TemperaturesCanvas), new PropertyMetadata(null));
    public static readonly DependencyProperty InteractivityConfigProperty = DependencyProperty.Register(nameof(InteractivityConfig), typeof(InteractivityConfig), typeof(TemperaturesCanvas), new PropertyMetadata(null));

    public event Action<BytePainter>? OnDrawingFinished;
    public event Action<int, int>? OnSizeChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsInteractivityEnabled
    {
        get { return (bool)GetValue(IsInteractivityEnabledProperty); }
        set { SetValue(IsInteractivityEnabledProperty, value); }
    }

    public Gradient Gradient
    {
        get { return (Gradient)GetValue(GradientProperty); }
        set { SetValue(GradientProperty, value); }
    }

    public InteractivityConfig InteractivityConfig
    {
        get { return (InteractivityConfig)GetValue(InteractivityConfigProperty); }
        set
        {
            SetValue(InteractivityConfigProperty, value);
        }
    }

    TemperaturesMatrix _matrix;
    public TemperaturesMatrix Matrix
    {
        get => _matrix;
        set
        {
            _matrix = value;

            widthTxtBox.Text = Matrix.Config.Width.ToString();
            heightTxtBox.Text = Matrix.Config.Height.ToString();
        }
    }

    BytePainter bytePainter;
    BytePainter? previewBytePainter;

    public TemperaturesCanvas()
    {
        InitializeComponent();
    }

    public void RenderMatrix(TemperaturesMatrix matrix)
    {
        Matrix = matrix;
        matrixImage.Source = BytePainter.ConvertBytesArrayToBitmap(matrix.getByteArray(), matrix.Config.Width, matrix.Config.Height, Gradient);
        bytePainter = new BytePainter(matrix.Config.Width, matrix.Config.Height, Gradient);
    }

    private void matrixImage_Loaded(object sender, RoutedEventArgs e)
    {
        bytePainter = new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
        matrixImage.Source = BytePainter.ConvertBytesArrayToBitmap(Matrix.getByteArray(), Matrix.Config.Width, Matrix.Config.Height, Gradient);

        widthTxtBox.Text = Matrix.Config.Width.ToString();
        heightTxtBox.Text = Matrix.Config.Height.ToString();
    }

    private Point getPosition(MouseEventArgs e)
    {
        var position = e.GetPosition(matrixImage);
        int xCoordinate = (int)(position.X / matrixImage.ActualWidth * Matrix.Config.Width);
        int yCoordinate = (int)(position.Y / matrixImage.ActualHeight * Matrix.Config.Height);

        return new Point(xCoordinate, yCoordinate);
    }

    Point? initialMouseCoords = null;

    private void matrixImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsInteractivityEnabled)
        {
            return;
        }

        initialMouseCoords = getPosition(e);
        previewBytePainter = new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
    }

    private void matrixImage_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (initialMouseCoords is Point initialPoint)
        {
            var currentMouseCoords = getPosition(e);

            switch (InteractivityConfig.Shape)
            {
                case InteractivityConfig.ShapeEnum.Rectangle:
                    bytePainter = new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
                    bytePainter.DrawRectangle(InteractivityConfig.Temperature, initialPoint, currentMouseCoords);
                    break;
                case InteractivityConfig.ShapeEnum.StraightLine:
                    bytePainter = new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
                    bytePainter.DrawLine(InteractivityConfig.Temperature, initialPoint, currentMouseCoords, InteractivityConfig.Thickness);
                    break;
                case InteractivityConfig.ShapeEnum.Circle:
                    bytePainter = new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
                    var radius = (int)Math.Sqrt(Math.Pow(initialPoint.X - currentMouseCoords.X, 2) + Math.Pow(initialPoint.Y - currentMouseCoords.Y, 2));
                    bytePainter.DrawCircle(InteractivityConfig.Temperature, initialPoint, radius);
                    break;
            }

            initialMouseCoords = null;
            drawingImage.Source = null;

            bytePainter.DrawFrame(0, 1);
            drawingImage.Source = null;

            OnDrawingFinished?.Invoke(bytePainter);
        }
    }

    private void matrixImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (initialMouseCoords is Point initialPoint)
        {
            var currentMouseCoords = getPosition(e);

            if (InteractivityConfig.Shape == InteractivityConfig.ShapeEnum.FreeForm)
            {
                bytePainter.DrawCircle(InteractivityConfig.Temperature, currentMouseCoords, InteractivityConfig.Thickness);
            }

            previewBytePainter.DrawLine(InteractivityConfig.Temperature, initialPoint, currentMouseCoords, 1);

            drawingImage.Source = previewBytePainter.Bitmap;
        }
    }

    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (matrixImage.IsLoaded)
        {
            matrixImage.Source = BytePainter.ConvertBytesArrayToBitmap(Matrix.getByteArray(), Matrix.Config.Width, Matrix.Config.Height, Gradient);

            new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
            drawingImage.Source = bytePainter.Bitmap;
        }
    }

    //handling the size inputs
    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !(e.Text.Length == 0 || e.Text.All(char.IsDigit));

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        OnSizeChanged?.Invoke(int.Parse(widthTxtBox.Text), int.Parse(heightTxtBox.Text));

        saveButton.Visibility = Visibility.Collapsed;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        int newWidth;
        var isNewWidthCorrect = int.TryParse(widthTxtBox.Text, out newWidth);

        int newHeight;
        var isNewHeightCorrect = int.TryParse(heightTxtBox.Text, out newHeight);

        saveButton.Visibility = (!isNewWidthCorrect || !isNewHeightCorrect || newWidth != Matrix.Config.Width || newHeight != Matrix.Config.Height) ? Visibility.Visible : Visibility.Collapsed;
        saveButton.IsEnabled = isNewWidthCorrect && isNewHeightCorrect && newWidth >= MinSize && newHeight >= MinSize;
    }
}