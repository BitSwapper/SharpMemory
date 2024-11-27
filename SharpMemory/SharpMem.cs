using System.Diagnostics;
using SharpMemory.Enums;
using SharpMemory.Interfaces;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;


public sealed class SharpMem : ISharpMem
{
    readonly static SharpMem sharpMemInst;
    public bool IsConnectedToProcess { get; }
    public string ProcessName { get; }
    public Process? Process { get; }
    public IntPtr ProcessHandle { get; private set; }

    public ReadFunctions ReadFuncs { get; private set; }
    public WriteFunctions WriteFuncs { get; private set; }
    public ModuleFunctions ModuleFuncs { get; init; }
    public PatternScanning PatternScanning { get; init; }
    public MemoryAnalyzer MemoryAnalyzer { get; } = new();
    public MemoryAllocator MemoryAllocator { get; } = new();

    bool _disposed;
    bool _hasInitializedEvents;

    public SharpMem(string processName, ProcessAccessFlags flags, Endianness endianness = Endianness.LittleEndian)
    {
        ProcessName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? processName[..^4]: processName;

        Process = GetProcess();
        if(Process is null)
            throw new ProcessNotFoundException($"Process '{ProcessName}' was not found.");

        ProcessHandle = OpenProcess((uint)flags, false, Process.Id);
        if(ProcessHandle == IntPtr.Zero)
            throw new ProcessAccessException($"Failed to get required access to process '{ProcessName}'.");

        IsConnectedToProcess = true;
        InitReadWriteFuncs(endianness);
        InitializeEvents();
        ModuleFuncs = new(this);
        PatternScanning = new PatternScanning(this);
        AddressExtensions.Initialize(this);
    }

    public void Dispose()
    {
        if(_disposed) return;

        Process?.Dispose();
        ReadFuncs = null!;
        WriteFuncs = null!;

        if(ProcessHandle != IntPtr.Zero)
        {
            CloseHandle(ProcessHandle);
            ProcessHandle = IntPtr.Zero;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public void SetEndianness(Endianness endianness)
    {
        ThrowIfDisposed();
        InitReadWriteFuncs(endianness);
    }

    Process? GetProcess()
    {
        try
        {
            return Process.GetProcessesByName(ProcessName)[0];
        }
        catch(IndexOutOfRangeException)
        {
            return null;
        }
    }

    void InitReadWriteFuncs(Endianness endianness)
    {
        ReadFuncs = new ReadFunctions(this, endianness);
        WriteFuncs = new WriteFunctions(this, endianness);
    }

    void InitializeEvents()
    {
        if(!_hasInitializedEvents)
        {
            WriteFuncs.WriteFailed += OnWriteFailed;
            _hasInitializedEvents = true;
        }
    }

    void OnWriteFailed()
    {
    }

    void ThrowIfDisposed()
    {
        if(_disposed)
            throw new ObjectDisposedException(nameof(SharpMem));
    }

    ~SharpMem()
    {
        Dispose();
    }
}

public class ProcessNotFoundException : Exception
{
    public ProcessNotFoundException(string message) : base(message) { }
}

public class ProcessAccessException : Exception
{
    public ProcessAccessException(string message) : base(message) { }
}