using System.Numerics;
using System.Text;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;
public class ReadFunctions
{
    public T Read<T>(Address address, bool useVirtualProtect = true)
    {
        var read = SharpMem.Inst.ReadFuncs.ReadByteArray;

        if(typeof(T).IsBasicType())
        {
            if(typeof(T)      == typeof(Byte))    return (T)(object)read(address.value, sizeof(Byte), useVirtualProtect)[0];
            else if(typeof(T) == typeof(Char))    return (T)(object)BitConverter.ToChar(read(address.value, sizeof(Char), useVirtualProtect));
            else if(typeof(T) == typeof(Boolean)) return (T)(object)BitConverter.ToBoolean(read(address.value, sizeof(Boolean), useVirtualProtect));
            else if(typeof(T) == typeof(Int16))   return (T)(object)BitConverter.ToInt16(read(address.value, sizeof(Int16), useVirtualProtect));
            else if(typeof(T) == typeof(Int32))   return (T)(object)BitConverter.ToInt32(read(address.value, sizeof(Int32), useVirtualProtect));
            else if(typeof(T) == typeof(Int64))   return (T)(object)BitConverter.ToInt64(read(address.value, sizeof(Int64), useVirtualProtect));
            else if(typeof(T) == typeof(UInt16))  return (T)(object)BitConverter.ToUInt16(read(address.value, sizeof(UInt16), useVirtualProtect));
            else if(typeof(T) == typeof(UInt32))  return (T)(object)BitConverter.ToUInt32(read(address.value, sizeof(UInt32), useVirtualProtect));
            else if(typeof(T) == typeof(UInt64))  return (T)(object)BitConverter.ToUInt64(read(address.value, sizeof(UInt64), useVirtualProtect));
            else if(typeof(T) == typeof(Single))  return (T)(object)BitConverter.ToSingle(read(address.value, sizeof(Single), useVirtualProtect));
            else if(typeof(T) == typeof(Double))  return (T)(object)BitConverter.ToDouble(read(address.value, sizeof(Double), useVirtualProtect));
        }


        if(typeof(T) == typeof(Vector2))
        {
            float x = BitConverter.ToSingle(read(address.value + 0, sizeof(Single), useVirtualProtect));
            float y = BitConverter.ToSingle(read(address.value + 4, sizeof(Single), useVirtualProtect));

            return (T)(object)new Vector2(x, y);
        }

        if(typeof(T) == typeof(Vector3))
        {
            float x = BitConverter.ToSingle(read(address.value + 0, sizeof(Single), useVirtualProtect));
            float y = BitConverter.ToSingle(read(address.value + 4, sizeof(Single), useVirtualProtect));
            float z = BitConverter.ToSingle(read(address.value + 8, sizeof(Single), useVirtualProtect));

            return (T)(object)new Vector3(x, y, z);
        }

        return (T)(object)null;
    }

    public byte[] ReadByteArray(long address, uint sizeToRead, bool useVirtualProtect = true)
    {
        if(!SharpMem.Inst.IsConnectedToProcess)
            throw new System.Exception();

        byte[] memoryBuffer = new byte[sizeToRead];

        VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, PAGE_READWRITE, out uint oldProtect);

        try
        {
            ReadProcessMemory(SharpMem.Inst.ProcessHandle, (IntPtr)address, memoryBuffer, sizeToRead, out uint bytesRead);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception while reading memory at 0x{address:X}: {ex.Message}");
            return new byte[0];
        }
        finally
        {
            VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, oldProtect, out _);
        }

        return memoryBuffer;
    }


    public string ReadStringAscii(long address, uint size) => Encoding.ASCII.GetString(ReadByteArray(address, size));
    public string ReadStringUnicode(long address, uint size) => Encoding.Unicode.GetString(ReadByteArray(address, size));
    public string ReadStringUTF8(long address, uint size) => Encoding.UTF8.GetString(ReadByteArray(address, size));
}