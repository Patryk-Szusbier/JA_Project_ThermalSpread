using System.Runtime.InteropServices;

namespace ThermalSpread.simulator;

public class VectorAsmSimulation : Simulation
{

    [DllImport("C:\\Users\\Patryk\\Documents\\GitHub\\JA_Project_ThermalSpread\\ThermalSpread\\x64\\Release\\ASMModule.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool _runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift);

    protected override bool runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift)
    {
        return _runSingleStep(readData, writeData, width, height, startColumn, endColumn, mul, shift);
    }

    public VectorAsmSimulation(TemperaturesMatrix temperaturesMatrix) : base(temperaturesMatrix) { }
}
