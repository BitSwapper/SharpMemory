using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using SharpMemory.Enums;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;
public class ReadFunctions
{
    Endianness Endianness;
    delegate object BasicTypeConverter(byte[] bytes);

    public ReadFunctions(Endianness endianness) => Endianness = endianness;
    Dictionary<Type, BasicTypeConverter> converters = new Dictionary<Type, BasicTypeConverter>
    {
        { typeof(Byte),    bytes => bytes[0] },
        { typeof(Char),    bytes => BitConverter.ToChar(bytes) },
        { typeof(Boolean), bytes => BitConverter.ToBoolean(bytes) },
        { typeof(Int16),   bytes => BitConverter.ToInt16(bytes) },
        { typeof(Int32),   bytes => BitConverter.ToInt32(bytes) },
        { typeof(Int64),   bytes => BitConverter.ToInt64(bytes) },
        { typeof(UInt16),  bytes => BitConverter.ToUInt16(bytes) },
        { typeof(UInt32),  bytes => BitConverter.ToUInt32(bytes) },
        { typeof(UInt64),  bytes => BitConverter.ToUInt64(bytes) },
        { typeof(Single),  bytes => BitConverter.ToSingle(bytes) },
        { typeof(Double),  bytes => BitConverter.ToDouble(bytes) },
    };


    public T Read<T>(Address address, bool useVirtualProtect = true)
    {
        var read = SharpMem.Inst.ReadFuncs.ReadByteArrayDefaultEndian;

        if(typeof(T).IsBasicType())
        {
            BasicTypeConverter converter = converters[typeof(T)];
            byte[] bytes = read(address.value, (uint)Marshal.SizeOf(typeof(T)), useVirtualProtect);
            return (T)converter(bytes);
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


    private byte[] reusableBuffer;  // Adjust the size as needed

        public Memory<byte> ReadByteArrayLittleEndian(long address, uint sizeToRead, bool useVirtualProtect = true)
        {
        if(reusableBuffer == null || reusableBuffer.Length == 0)
            reusableBuffer = new byte[512 * 1024 * 1024];

        if(!SharpMem.Inst.IsConnectedToProcess)
                throw new System.Exception();

            Memory<byte> memoryBuffer = new Memory<byte>(reusableBuffer);
            uint oldProtect = 0;

            if(useVirtualProtect)
                VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, PAGE_READWRITE, out oldProtect);

            try
            {
                ReadProcessMemory(SharpMem.Inst.ProcessHandle, (IntPtr)address, reusableBuffer, sizeToRead, out uint bytesRead);
                memoryBuffer = memoryBuffer.Slice(0, (int)bytesRead);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception while reading memory at 0x{address:X}: {ex.Message}");
                return Memory<byte>.Empty;
            }
            finally
            {
                if(useVirtualProtect)
                    VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, oldProtect, out _);
            }

            return memoryBuffer;
        }


    public void ClearBuffer()
    {
        reusableBuffer = null;
    }


    public byte[] ReadByteArrayDefaultEndian(long address, uint sizeToRead, bool useVirtualProtect = true)
    {
        if(!SharpMem.Inst.IsConnectedToProcess)
            throw new System.Exception();

        byte[] memoryBuffer = new byte[sizeToRead];
        uint oldProtect = 0;
        if(useVirtualProtect)
            VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, PAGE_READWRITE, out oldProtect);

        try
        {
            ReadProcessMemory(SharpMem.Inst.ProcessHandle, (IntPtr)address, memoryBuffer, sizeToRead, out uint bytesRead);

            if(Endianness == Endianness.BigEndian)
            {
                // Reverse only the portion of the array that was read
                Array.Reverse(memoryBuffer, 0, (int)sizeToRead);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception while reading memory at 0x{address:X}: {ex.Message}");
            return new byte[0];
        }
        finally
        {
            if(useVirtualProtect)
                VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, oldProtect, out _);
        }

        return memoryBuffer;
    }



    public string ReadStringAscii(long address, uint size) => Encoding.ASCII.GetString(ReadByteArrayDefaultEndian(address, size));
    public string ReadStringUnicode(long address, uint size) => Encoding.Unicode.GetString(ReadByteArrayDefaultEndian(address, size));
    public string ReadStringUTF8(long address, uint size) => Encoding.UTF8.GetString(ReadByteArrayDefaultEndian(address, size));
}