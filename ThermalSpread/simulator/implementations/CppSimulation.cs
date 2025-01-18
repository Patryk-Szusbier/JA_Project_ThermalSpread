using System.Runtime.InteropServices;

namespace ThermalSpread.simulator.implementations;

public class CppSimulation : Simulation
{
    [DllImport("C:\\Users\\Patryk\\Documents\\GitHub\\JA_Project_ThermalSpread\\ThermalSpread\\x64\\Release\\CppModule.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool _runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift);

    protected override bool runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift)
    {
        return _runSingleStep(readData, writeData, width, height, startColumn, endColumn, mul, shift);
    }

    public CppSimulation(TemperaturesMatrix temperaturesMatrix) : base(temperaturesMatrix) { }
    
}
