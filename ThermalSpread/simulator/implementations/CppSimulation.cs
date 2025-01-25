using System.IO;
using System.Runtime.InteropServices;

namespace ThermalSpread.simulator.implementations;

public class CppSimulation : Simulation
{
    private const string DllName = "CPPModule.dll";

    // Import funkcji LoadLibrary z kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool _runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift);
    static CppSimulation()
    {
        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64", "Release", DllName);
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"Nie znaleziono pliku DLL: {dllPath}");
        }

        IntPtr handle = LoadLibrary(dllPath);
        if (handle == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(errorCode, $"Nie udało się załadować DLL: {dllPath}");
        }
    }
    protected override bool runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift)
    {
        return _runSingleStep(readData, writeData, width, height, startColumn, endColumn, mul, shift);
    }

    public CppSimulation(TemperaturesMatrix temperaturesMatrix) : base(temperaturesMatrix) { }
    
}
