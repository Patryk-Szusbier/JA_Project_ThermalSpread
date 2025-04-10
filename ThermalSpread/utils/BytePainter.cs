﻿using ThermalSpread.simulator;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Point = System.Drawing.Point;

namespace ThermalSpread.utils;

public class BytePainter
{
    private static Color interpolateBetweenColors(Gradient gradient, double factor)
    {
        if (factor < 0.0)
        {
            factor = 0.0;
        }
        else if (factor > 1.0)
        {
            factor = 1.0;
        }

        return Color.FromRgb(
            (byte)(gradient.From.R + factor * (gradient.To.R - gradient.From.R)),
            (byte)(gradient.From.G + factor * (gradient.To.G - gradient.From.G)),
            (byte)(gradient.From.B + factor * (gradient.To.B - gradient.From.B))
        );
    }

    public static WriteableBitmap ConvertBytesArrayToBitmap(byte[] byteArray, int width, int height, Gradient gradient)
    {
        var writableImg = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

        var pixelData = new byte[width * height * 4];

        for (var i = 0; i < byteArray.Length; i++)
        {
            var pixelValue = byteArray[i] / (double)Constants.MaxTemperature;
            var color = interpolateBetweenColors(gradient, pixelValue);

            var pixelIndex = i * 4;
            pixelData[pixelIndex] = color.B;
            pixelData[pixelIndex + 1] = color.G;
            pixelData[pixelIndex + 2] = color.R;
            pixelData[pixelIndex + 3] = byte.MaxValue;
        }

        writableImg.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

        return writableImg;
    }

    private int getIndex(int x, int y) => y * Width + x;
    private Point getPoint(int index) => new Point(index % Width, index / Width);

    private bool isXInRange(int x) => x >= 0 && x < Width;
    private bool isYInRange(int y) => y >= 0 && y < Height;
    private bool isInRange(int x, int y) => isXInRange(x) && isYInRange(y);

    public int Width { get; }
    public int Height { get; }
    public Gradient Gradient { get; private set; }
    public WriteableBitmap Bitmap { get; private set; }
    public byte[] BytesArray { get; private set; }
    public IEnumerable<(Point point, byte value)> GetChanges()
    {
        for (var index = 0; index < BytesArray.Length; index += 1)
        {
            if (BytesArray[index] != 0)
            {
                yield return (getPoint(index), BytesArray[index]);
            }
        }
    }

    public BytePainter(int width, int height, Gradient gradient)
    {
        Width = width;
        Height = height;
        Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        BytesArray = new byte[width * height];
        Gradient = gradient;
    }

    public void DrawPixel(byte value, int x, int y)
    {
        if (isInRange(x, y))
        {
            var color = interpolateBetweenColors(Gradient, value / (double)byte.MaxValue);
            var colorData = new byte[] { color.B, color.G, color.R, color.A };

            BytesArray[getIndex(x, y)] = value;
            Bitmap.WritePixels(new Int32Rect(x, y, 1, 1), colorData, 4, 0);
        }
    }

    public void DrawRectangle(byte value, Point startCoords, Point endCoords)
    {
        var color = interpolateBetweenColors(Gradient, value / (double)byte.MaxValue);
        var colorData = new byte[] { color.B, color.G, color.R, color.A };

        var leftUpperPoint = new Point(Math.Min(startCoords.X, endCoords.X), Math.Min(startCoords.Y, endCoords.Y));
        var rightLowerPoint = new Point(Math.Max(startCoords.X, endCoords.X), Math.Max(startCoords.Y, endCoords.Y));

        for (var x = leftUpperPoint.X; x <= rightLowerPoint.X; x += 1)
        {
            if (!isXInRange(x))
            {
                return;
            }

            for (var y = leftUpperPoint.Y; y <= rightLowerPoint.Y; y += 1)
            {
                if (!isYInRange(y))
                {
                    break;
                }

                BytesArray[getIndex(x, y)] = value;
                Bitmap.WritePixels(new Int32Rect(x, y, 1, 1), colorData, 4, 0);
            }
        }
    }

    public void DrawCircle(byte value, Point centerCoords, int radius)
    {
        void drawHorizontalLine(int startX, int endX, int y)
        {
            for (int x = startX; x <= endX; x++)
            {
                DrawPixel(value, x, y);
            }
        }

        var x = 0;
        var y = radius;
        var p = 3 - 2 * radius;

        while (x <= y)
        {
            // Draw the horizontal line between two points to fill the circle
            drawHorizontalLine(centerCoords.X - x, centerCoords.X + x, centerCoords.Y + y);
            drawHorizontalLine(centerCoords.X - x, centerCoords.X + x, centerCoords.Y - y);

            drawHorizontalLine(centerCoords.X - y, centerCoords.X + y, centerCoords.Y + x);
            drawHorizontalLine(centerCoords.X - y, centerCoords.X + y, centerCoords.Y - x);

            x++;

            if (p < 0)
                p += 4 * x + 6;
            else
            {
                y--;
                p += 4 * (x - y) + 10;
            }
        }
    }

    public void DrawFrame(byte value, int thickness)
    {
        DrawLine(value, new Point(0, 0), new Point(Width - 1, 0), thickness);                                 //top edge
        DrawLine(value, new Point(0, Height - 1), new Point(Width - 1, Height - 1), thickness);           //bottom edge
        DrawLine(value, new Point(0, 0), new Point(0, Height - 1), thickness);                                //left edge
        DrawLine(value, new Point(Width - 1, 0), new Point(Width - 1, Height - 1), thickness);            //right edge
    }

    public void DrawLine(byte value, Point startCoords, Point endCoords, int thickness)
    {
        var x1 = startCoords.X;
        var y1 = startCoords.Y;
        var x2 = endCoords.X;
        var y2 = endCoords.Y;

        void drawFilledRectangle(int x, int y, int thickness)
        {
            for (int i = x; i < x + thickness; i++)
            {
                for (int j = y; j < y + thickness; j++)
                {
                    DrawPixel(value, i, j);
                }
            }
        }

        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            drawFilledRectangle(x1 - thickness / 2, y1 - thickness / 2, thickness);

            if (x1 == x2 && y1 == y2)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }
    }
}