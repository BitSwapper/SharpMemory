using System.Diagnostics;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;
public class SharpMem
{
    public bool IsConnectedToProcess { get; private set; } = false;
    public string? ProcessName { get; private set; }
    public Process? Process { get; private set; }
    public IntPtr ProcessHandle { get; private set; }

    public ReadFunctions ReadFuncs { get; } = new();
    public WriteFunctions WriteFuncs { get; } = new();
    public ModuleFunctions ModuleFuncs { get; } = new();
    public PatternScanning PatternScanning { get; } = new();
    public MemoryAnalyzer MemoryAnalyzer { get; } = new();
    public MemoryAllocator MemoryAllocator { get; } = new();
    public TrampolineHook TrampolineHook { get; } = new();



    public bool Initialize(string processName, ProcessAccessFlags flags)
    {
        ProcessName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                      ? processName.Substring(0, processName.Length - 4)
                      : processName;

        IsConnectedToProcess = OpenProc(flags);

        return IsConnectedToProcess;
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

    static readonly SharpMem instance = new();
    public static SharpMem Inst => instance;
}