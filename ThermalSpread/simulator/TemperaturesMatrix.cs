using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Point = System.Drawing.Point;
using Temperature = System.Byte;

namespace ThermalSpread.simulator;

public class TemperaturesMatrix : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private TemperaturesMatrix(Config config, Temperature[] matrix)
    {
        Config = config;
        this.matrix = matrix;
    }

    private int getIndex(Point point)
    {
        return point.Y * Config.Width + point.X;
    }

    public Config Config { get; set; }
    readonly Temperature[] matrix;

    public TemperaturesMatrix(Config config)
    {
        if (!config.isCorrect())
        {
            throw new ArgumentException("Provided invalid Config object");
        }

        Config = config;
        matrix = Enumerable.Repeat(Constants.MinTemperature, config.Width * config.Height).ToArray();

        foreach (var (point, temperature) in config.InitialTemperatures)
        {
            matrix[getIndex(point)] = temperature;
        }

        foreach (var (point, temperature) in config.ConstantTemperatures)
        {
            matrix[getIndex(point)] = temperature;
        }
    }

    public Temperature[] getByteArray()
    {
        return matrix;
    }

    public void setPoint(Temperature temperature, Point point)
    {
        matrix[getIndex(point)] = temperature;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Matrix)));
    }

    public TemperaturesMatrix Clone()
    {
        var matrixCopy = new Temperature[matrix.Length];
        Array.Copy(matrix, matrixCopy, matrix.Length);

        return new TemperaturesMatrix(Config, matrixCopy);
    }

    public void setPoints(IEnumerable<(Point point, byte value)> records)
    {
        foreach (var (point, temperature) in records)
        {
            matrix[getIndex(point)] = temperature;
        }
    }
}

