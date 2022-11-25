using System.Numerics;
using System.Text;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;
public class ReadFunctions
{
    public T Read<T>(Address address)
    {
        var read = SharpMem.Inst.ReadFuncs.ReadByteArray;

        if(TypeHelper.IsBasicType(typeof(T)))
        {
            if(typeof(T) == typeof(Byte)) return (T)(object)read(address.value, sizeof(Byte))[0];

            if(typeof(T) == typeof(Char)) return (T)(object)BitConverter.ToChar(read(address.value, sizeof(Char)));

            if(typeof(T) == typeof(Boolean)) return (T)(object)BitConverter.ToBoolean(read(address.value, sizeof(Boolean)));

            if(typeof(T) == typeof(Int16)) return (T)(object)BitConverter.ToInt16(read(address.value, sizeof(Int16)));

            if(typeof(T) == typeof(Int32)) return (T)(object)BitConverter.ToInt32(read(address.value, sizeof(Int32)));

            if(typeof(T) == typeof(Int64)) return (T)(object)BitConverter.ToInt64(read(address.value, sizeof(Int64)));

            if(typeof(T) == typeof(UInt16)) return (T)(object)BitConverter.ToUInt16(read(address.value, sizeof(UInt16)));

            if(typeof(T) == typeof(UInt32)) return (T)(object)BitConverter.ToUInt32(read(address.value, sizeof(UInt32)));

            if(typeof(T) == typeof(UInt64)) return (T)(object)BitConverter.ToUInt64(read(address.value, sizeof(UInt64)));

            if(typeof(T) == typeof(Single)) return (T)(object)BitConverter.ToSingle(read(address.value, sizeof(Single)));

            if(typeof(T) == typeof(Double)) return (T)(object)BitConverter.ToDouble(read(address.value, sizeof(Double)));
        }


        if(typeof(T) == typeof(Vector2))
        {
            float x = BitConverter.ToSingle(read(address.value + 0, sizeof(Single)));
            float y = BitConverter.ToSingle(read(address.value + 4, sizeof(Single)));

            return (T)(object)new Vector2(x, y);
        }

        if(typeof(T) == typeof(Vector3))
        {
            float x = BitConverter.ToSingle(read(address.value + 0, sizeof(Single)));
            float y = BitConverter.ToSingle(read(address.value + 4, sizeof(Single)));
            float z = BitConverter.ToSingle(read(address.value + 8, sizeof(Single)));

            return (T)(object)new Vector3(x, y, z);
        }

        return (T)(object)null;
    }

    public byte[] ReadByteArray(long address, uint sizeToRead)
    {
        if(!SharpMem.Inst.IsConnectedToProcess)
            return Array.Empty<byte>();

        byte[] memoryBuffer = new byte[sizeToRead];
        try { ReadProcessMemory(SharpMem.Inst.ProcessHandle, (IntPtr)address, memoryBuffer, sizeToRead, out uint dummy); }
        catch { return new byte[0]; }
        return memoryBuffer;
    }
    
    public string ReadStringAscii(long address, uint size) => Encoding.ASCII.GetString(ReadByteArray(address, size));
    public string ReadStringUnicode(long address, uint size) => Encoding.Unicode.GetString(ReadByteArray(address, size));
    public string ReadStringUTF8(long address, uint size) => Encoding.UTF8.GetString(ReadByteArray(address, size));
}