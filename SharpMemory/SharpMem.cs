using System.Diagnostics;
using SharpMemory.Enums;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;

public class SharpMem : IDisposable
{
    static readonly SharpMem instance = new();
    public static SharpMem Inst => instance;

    public bool IsConnectedToProcess { get; private set; } = false;
    public string? ProcessName { get; private set; }
    public Process? Process { get; private set; }
    public IntPtr ProcessHandle { get; private set; }

    public ReadFunctions ReadFuncs { get; private set; }
    public WriteFunctions WriteFuncs { get; private set; }
    public ModuleFunctions ModuleFuncs { get; private set; } = new();
    public PatternScanning PatternScanning { get; private set; } = new();
    public MemoryAnalyzer MemoryAnalyzer { get; private set; } = new();
    public MemoryAllocator MemoryAllocator { get; private set; } = new();
    //public TrampolineHook TrampolineHook { get; private set; } = new();

    bool hasInitializedEvents = false;

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);
    public void Dispose()
    {

        Process?.Dispose();
        ReadFuncs = null;
        WriteFuncs = null;
        ModuleFuncs = null;
        PatternScanning = null;
        MemoryAnalyzer = null;
        MemoryAllocator = null;

        if(ProcessHandle != IntPtr.Zero)
        {
            CloseHandle(ProcessHandle);
            ProcessHandle = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }

    public void SetToBigEndian() => InitReadWriteFuncs(Endianness.BigEndian);

    public void SetLittleEndian() => InitReadWriteFuncs(Endianness.LittleEndian);

    public bool Initialize(string processName, ProcessAccessFlags flags, Endianness endianness)
    {
        ProcessName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                      ? processName.Substring(0, processName.Length - 4)
                      : processName;

        IsConnectedToProcess = OpenProc(flags);

        InitReadWriteFuncs(endianness);

        if(!hasInitializedEvents)
            WriteFuncs.WriteFailed += OnWriteFailed;

        hasInitializedEvents = true;

        return IsConnectedToProcess;
    }

    void InitReadWriteFuncs(Endianness endianness)
    {
        ReadFuncs = new(endianness);
        WriteFuncs = new(endianness);
    }

    private bool OpenProc(ProcessAccessFlags flags)
    {
        try { Process = Process.GetProcessesByName(ProcessName)[0]; }
        catch { Process = null; }

        if(Process != null)
            ProcessHandle = OpenProcess((uint)flags, false, Process.Id);
        else
            return false;
        return true;
    }

    void OnWriteFailed()
    {
        IsConnectedToProcess = false;
        ProcessHandle = default;
        Process = null;
    }



    ~SharpMem()
    {
        Dispose();
    }


}
