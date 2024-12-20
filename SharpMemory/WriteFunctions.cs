﻿using System.Runtime.InteropServices;
using System.Text;
using SharpMemory.Enums;
using static SharpMemory.Native.NativeData;
namespace SharpMemory;
public class WriteFunctions
{
    public event Action WriteFailed;
    Endianness Endianness;
    SharpMem SharpMem { get; init; }
    public WriteFunctions(SharpMem sharpMem, Endianness endianness)
    {
        SharpMem = sharpMem;
        Endianness = endianness;
    }

    public bool Write<T>(Address address, T value, bool useVirtualProtect = false)
    {
        if(value == null)
            return false;

        int size = 0;
        byte[] byteDataToWrite;

        if(typeof(T) == typeof(bool))
            byteDataToWrite = BitConverter.GetBytes((bool)(object)value);
        else if(typeof(T) == typeof(char))
            byteDataToWrite = BitConverter.GetBytes((char)(object)value);
        else
        {
            size = Marshal.SizeOf(typeof(T));
            byteDataToWrite = new byte[size];

            GCHandle gcHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
            Marshal.Copy(gcHandle.AddrOfPinnedObject(), byteDataToWrite, 0, size);
            gcHandle.Free();
        }

        return byteDataToWrite == null ? false : WriteByteArray(address.value, byteDataToWrite, useVirtualProtect);
    }

    public bool WriteByteArray(long address, byte[] value, bool useVirtualProtect = false)
    {
        if(!SharpMem.IsConnectedToProcess)
            return false;

        var procHandle = SharpMem.ProcessHandle;
        bool bResult = false;
        uint ogPageProtection = 0;

        try
        {
            if(Endianness == Endianness.BigEndian)
                Array.Reverse(value);

            if(useVirtualProtect) VirtualProtectEx(procHandle, (IntPtr)address, (uint)value.Length, PAGE_READWRITE, out ogPageProtection);

            bResult = WriteProcessMemory(procHandle, (IntPtr)address, value, (uint)value.Length, out uint bytesWritten);

            if(useVirtualProtect) VirtualProtectEx(procHandle, (IntPtr)address, (uint)value.Length, ogPageProtection, out uint _);
        }
        catch { bResult = false; }
        finally
        {
            if(!bResult)
                WriteFailed?.Invoke();
        }
        return bResult;
    }

    public bool WriteStringAscii(long address, string text, bool useVirtualProtect = false) => WriteByteArray(address, Encoding.ASCII.GetBytes(text), useVirtualProtect);

    public bool WriteStringUnicode(long address, string text, bool useVirtualProtect = false) => WriteByteArray(address, Encoding.Unicode.GetBytes(text), useVirtualProtect);

    public bool WriteNop(long address, int numOfBytes, bool useVirtualProtect = false)
    {
        byte[] nopBuffer = new byte[numOfBytes];
        for(int i = 0; i < numOfBytes; i++)
            nopBuffer[i] = 0x90;

        return WriteByteArray(address, nopBuffer, useVirtualProtect);
    }
}
