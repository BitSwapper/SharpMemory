using System.Runtime.InteropServices;
using static SharpMemory.MemoryAnalyzer;

namespace SharpMemory.Native;
public static class NativeData
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x1F0FFF,
        Terminate = 0x1,
        CreateThread = 0x2,
        VMOperation = 0x8,
        VMRead = 0x10,
        VMWrite = 0x20,
        DuplicateHandle = 0x40,
        SetInformation = 0x200,
        QueryInformation = 0x400,
        ReadControl = 0x20000,
        Synchronize = 0x100000
    }

    public readonly static uint PAGE_READWRITE = 0x04;

    [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint desiredAccess, bool bInheritHandle, int processId);
    [DllImport("kernel32.dll")] public static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr baseAddress, byte[] byteArrayBuffer, uint size, out uint numberOfBytesRead);
    [DllImport("kernel32.dll")] public static extern bool WriteProcessMemory(IntPtr processHandle, IntPtr baseAddress, byte[] byteArrayBuffer, uint size, out uint numberOfBytesRead);
    [DllImport("kernel32.dll")] public static extern bool VirtualProtectEx(IntPtr processHandle, IntPtr baseAddress, uint size, uint newProtection, out uint oldProtection);
    [DllImport("kernel32.dll")] public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
}