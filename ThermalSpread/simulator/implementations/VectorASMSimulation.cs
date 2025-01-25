using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace ThermalSpread.simulator
{
    public class VectorAsmSimulation : Simulation
    {
        private static string dllName;

        // Import funkcji LoadLibrary z kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private static IntPtr _dllHandle;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool RunSingleStepDelegate(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift);

        private static RunSingleStepDelegate _runSingleStepFunc;

        static VectorAsmSimulation()
        {
            // Wykryj wsparcie dla AVX-512
            bool isAvx512Supported = CheckAvx512Support();

            // Wybierz odpowiednią bibliotekę DLL
            dllName = isAvx512Supported ? "ASMModule.dll" : "ASMModule2.dll";
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64", "Release", dllName);

            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException($"Nie znaleziono pliku DLL: {dllPath}");
            }

            _dllHandle = LoadLibrary(dllPath);
            if (_dllHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode, $"Nie udało się załadować DLL: {dllPath}");
            }

            IntPtr runSingleStepPtr = GetProcAddress(_dllHandle, "_runSingleStep");
            if (runSingleStepPtr == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode, $"Nie udało się znaleźć funkcji '_runSingleStep' w DLL: {dllPath}");
            }

            _runSingleStepFunc = (RunSingleStepDelegate)Marshal.GetDelegateForFunctionPointer(runSingleStepPtr, typeof(RunSingleStepDelegate));
        }

        protected override bool runSingleStep(byte[] readData, byte[] writeData, int width, int height, int startColumn, int endColumn, ushort mul, int shift)
        {
            return _runSingleStepFunc(readData, writeData, width, height, startColumn, endColumn, mul, shift);
        }

        private static bool CheckAvx512Support()
        {
            // Sprawdź wsparcie dla AVX-512 za pomocą klasy System.Runtime.Intrinsics.X86
            return Avx512F.IsSupported;
        }

        public VectorAsmSimulation(TemperaturesMatrix temperaturesMatrix) : base(temperaturesMatrix) { }
    }
}
