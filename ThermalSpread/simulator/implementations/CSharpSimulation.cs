namespace ThermalSpread.simulator;
public class CSharpSimulation : Simulation
{
    protected override bool runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift)
    {
        bool IsFinished = true;

        var iterator = 0;
        for (int row = 1; row <= height; row++)
        {
            iterator += width;

            for (int col = startColumn; col <= endColumn; col++)
            {
                var centerIndex = iterator + col;

                byte centerValue = readData[centerIndex];
                byte leftValue = readData[centerIndex - 1];
                byte rightValue = readData[centerIndex + 1];
                byte topValue = readData[centerIndex - width];
                byte bottomValue = readData[centerIndex + width];

                byte newValue = (byte)(((centerValue + leftValue + rightValue + topValue + bottomValue) * mul) >> shift);

                writeData[centerIndex] = newValue;

                if (newValue > 0)
                {
                    IsFinished = false;
                }
            }
        }

        return IsFinished;
    }

    public CSharpSimulation(TemperaturesMatrix temperaturesMatrix) : base(temperaturesMatrix) { }
}
