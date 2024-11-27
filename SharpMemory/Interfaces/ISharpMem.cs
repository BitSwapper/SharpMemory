using System.Diagnostics;
using SharpMemory.Enums;

namespace SharpMemory.Interfaces;

public interface ISharpMem : IDisposable
{
    bool IsConnectedToProcess { get; }
    string ProcessName { get; }
    Process? Process { get; }
    IntPtr ProcessHandle { get; }

    ReadFunctions ReadFuncs { get; }
    WriteFunctions WriteFuncs { get; }
    ModuleFunctions ModuleFuncs { get; }
    PatternScanning PatternScanning { get; }
    MemoryAnalyzer MemoryAnalyzer { get; }
    MemoryAllocator MemoryAllocator { get; }

    void SetEndianness(Endianness endianness);
}