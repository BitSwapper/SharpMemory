using System.Collections.Generic;
using System.Net;
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using static SharpMemory.Native.NativeData;
using System.Diagnostics;

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

    public Dictionary<long, long> FindUnknownMemory(ProcessModule[] allModules, IntPtr hProcess)
    {
        Dictionary<long, long> unknownMemory = new Dictionary<long, long>();
        Dictionary<long, long> knownModules = new Dictionary<long, long>();

        foreach(ProcessModule module in allModules)
        {
            knownModules.Add((long)module.BaseAddress, (long)module.BaseAddress + module.ModuleMemorySize);
        }

        IntPtr currentAddress = IntPtr.Zero;
        long currentStartAddress = 0;
        long currentEndAddress = 0;

        while(VirtualQueryEx(hProcess, currentAddress, out MEMORY_BASIC_INFORMATION memInfo, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) > 0)
        {
            long startAddress = (long)memInfo.BaseAddress;
            long endAddress = startAddress + (long)memInfo.RegionSize;

            if(memInfo.State == 0x1000 && !IsInKnownModules(knownModules, startAddress, endAddress))
            {
                // Memory region is committed and does not belong to any known modules
                if(currentStartAddress == 0)
                {
                    // Start of a new unknown memory region
                    currentStartAddress = startAddress;
                    currentEndAddress = endAddress;
                }
                else
                {
                    // Extend the current unknown memory region
                    currentEndAddress = endAddress;
                }
            }
            else
            {
                // End of the unknown memory region
                if(currentStartAddress != 0)
                {
                    unknownMemory.Add(currentStartAddress, currentEndAddress);
                    currentStartAddress = 0;
                    currentEndAddress = 0;
                }
            }

            currentAddress = (IntPtr)(startAddress + (long)memInfo.RegionSize);
        }

        if(currentStartAddress != 0)
            unknownMemory.Add(currentStartAddress, currentEndAddress);

        return unknownMemory;
    }

    private static bool IsInKnownModules(Dictionary<long, long> knownModules, long startAddress, long endAddress)
    {
        foreach(var kvp in knownModules)
            if(startAddress >= kvp.Key && endAddress <= kvp.Value)
                return true;

        return false;
    }
}
