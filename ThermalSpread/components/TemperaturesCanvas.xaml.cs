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
        //bytePainter = new BytePainter(Matrix.Config.Width, Matrix.Config.Height, Gradient);
        //matrixImage.Source = BytePainter.ConvertBytesArrayToBitmap(Matrix.getByteArray(), Matrix.Config.Width, Matrix.Config.Height, Gradient);

        //widthTxtBox.Text = Matrix.Config.Width.ToString();
        //heightTxtBox.Text = Matrix.Config.Height.ToString();
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