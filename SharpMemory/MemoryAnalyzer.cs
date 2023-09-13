using System.Collections.Generic;
using System.Net;
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;

public class MemoryAnalyzer
{

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    public IntPtr FindUnusedMemory(IntPtr hProcess, uint size, long inputAddress)
    {
        IntPtr currentAddress = (IntPtr)(inputAddress - int.MaxValue);
        while(true)
        {
            MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();
            int memInfoSize = VirtualQueryEx(hProcess, currentAddress, out memInfo, (uint)Marshal.SizeOf(memInfo));

            if(memInfoSize == 0)
                break;

            if(memInfo.Protect == 0x0 && (uint)memInfo.RegionSize.ToInt64() >= size)
            {
                // Find the first address of an unused region of at least the specified size
                IntPtr freeMemoryAddress = memInfo.BaseAddress;
                while((uint)memInfo.RegionSize.ToInt64() >= size)
                {
                    if(memInfo.Protect != 0x0)
                    {
                        if(Math.Abs(freeMemoryAddress.ToInt64() - inputAddress) <= int.MaxValue)
                        {
                            return freeMemoryAddress;
                        }
                    }
                    freeMemoryAddress = new IntPtr(freeMemoryAddress.ToInt64() + 1);
                    memInfoSize = VirtualQueryEx(hProcess, freeMemoryAddress, out memInfo, (uint)Marshal.SizeOf(memInfo));
                }
            }
            currentAddress = new IntPtr(currentAddress.ToInt64() + memInfo.RegionSize.ToInt64());
        }
        return IntPtr.Zero;
    }
}
