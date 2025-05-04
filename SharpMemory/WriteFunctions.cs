using System.Numerics;
using System.Runtime.InteropServices;
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
        if(value == null || address == 0)
            return false;

        int size = 0;
        byte[] byteDataToWrite = null;

        if(typeof(T) == typeof(bool))
            byteDataToWrite = BitConverter.GetBytes((bool)(object)value);
        else if(typeof(T) == typeof(char))
            byteDataToWrite = BitConverter.GetBytes((char)(object)value);
        else if(typeof(T) == typeof(Vector2))
            return WriteVector2(address.value, (Vector2)(object)value, useVirtualProtect);
        else if(typeof(T) == typeof(Vector3))
            return WriteVector3(address.value, (Vector3)(object)value, useVirtualProtect);
        else if(typeof(T) == typeof(Vector4))
            return WriteVector4(address.value, (Vector4)(object)value, useVirtualProtect);


        try//Generic approach for other structures using Marshal
        {
            size = Marshal.SizeOf(typeof(T));
            byteDataToWrite = new byte[size];

            GCHandle gcHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
            Marshal.Copy(gcHandle.AddrOfPinnedObject(), byteDataToWrite, 0, size);
            gcHandle.Free();

            return WriteByteArray(address.value, byteDataToWrite, useVirtualProtect);
        }
        catch { }

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

    private bool WriteVector2(long address, Vector2 value, bool useVirtualProtect = false)
    {
        byte[] buffer = new byte[sizeof(float) * 2];
        Buffer.BlockCopy(BitConverter.GetBytes(value.X), 0, buffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(value.Y), 0, buffer, 4, 4);
        return WriteByteArray(address, buffer, useVirtualProtect);
    }

    private bool WriteVector3(long address, Vector3 value, bool useVirtualProtect = false)
    {
        byte[] buffer = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(value.X), 0, buffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(value.Y), 0, buffer, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(value.Z), 0, buffer, 8, 4);
        return WriteByteArray(address, buffer, useVirtualProtect);
    }

    private bool WriteVector4(long address, Vector4 value, bool useVirtualProtect = false)
    {
        byte[] buffer = new byte[sizeof(float) * 4];
        Buffer.BlockCopy(BitConverter.GetBytes(value.X), 0, buffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(value.Y), 0, buffer, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(value.Z), 0, buffer, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(value.W), 0, buffer, 12, 4);
        return WriteByteArray(address, buffer, useVirtualProtect);
    }

    // handle writing generic struct types by reflection
    public bool WriteStruct<T>(Address address, T value, bool useVirtualProtect = false) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(value, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            return WriteByteArray(address.value, bytes, useVirtualProtect);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
