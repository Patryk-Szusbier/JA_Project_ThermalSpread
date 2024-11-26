using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ThermalSpread.simulator;

public class Config : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Width { get; set; }
    public int Height { get; set; }
    public IEnumerable<(Point point, byte value)> InitialTemperatures { get; set; }
    public IEnumerable<(Point point, byte value)> ConstantTemperatures { get; set; }
    public int AlfaCoeff { get; set; }

    public Config(int width, int height, IEnumerable<(Point point, byte value)> initialTemperatures, IEnumerable<(Point point, byte value)> constantTemperatures, int alfaCoeff)
    {
        Width = width;
        Height = height;
        InitialTemperatures = initialTemperatures;
        ConstantTemperatures = constantTemperatures;
        AlfaCoeff = alfaCoeff;
    }

    private bool isCoordCorrect(Point point) => point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
    private bool isInFrame(Point point) => point.X > 0 && point.X < Width - 1 && point.Y > 0 && point.Y < Height - 1;

    public bool isCorrect()
    {
        if (Width < 3 || Height < 3)
        {
            return false;
        }

        foreach (var (point, temperature) in InitialTemperatures)
        {
            if (!isCoordCorrect(point) || (temperature > 0 && !isInFrame(point))) { return false; }
        }

        foreach (var (point, temperature) in ConstantTemperatures)
        {
            if (!isCoordCorrect(point) || (temperature > 0 && !isInFrame(point))) { return false; }
        }

        return true;
    }
}
