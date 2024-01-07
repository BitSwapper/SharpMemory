using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using SharpMemory.Enums;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;
public class ReadFunctions
{
    Endianness Endianness;
    delegate object BasicTypeConverter(Memory<byte> bytes);

    const int SIZE_512mb = 512 * 1024 * 1024;

    public ReadFunctions(Endianness endianness) => Endianness = endianness;
    Dictionary<Type, BasicTypeConverter> basicTypeConverterDict = new Dictionary<Type, BasicTypeConverter>
    {
        { typeof(Byte),    bytes => bytes.Span[0] },
        { typeof(Char),    bytes => BitConverter.ToChar(   bytes.Span) },
        { typeof(Boolean), bytes => BitConverter.ToBoolean(bytes.Span) },
        { typeof(Int16),   bytes => BitConverter.ToInt16(  bytes.Span) },
        { typeof(Int32),   bytes => BitConverter.ToInt32(  bytes.Span) },
        { typeof(Int64),   bytes => BitConverter.ToInt64(  bytes.Span) },
        { typeof(UInt16),  bytes => BitConverter.ToUInt16( bytes.Span) },
        { typeof(UInt32),  bytes => BitConverter.ToUInt32( bytes.Span) },
        { typeof(UInt64),  bytes => BitConverter.ToUInt64( bytes.Span) },
        { typeof(Single),  bytes => BitConverter.ToSingle( bytes.Span) },
        { typeof(Double),  bytes => BitConverter.ToDouble( bytes.Span) },
    };



    public T Read<T>(Address address, bool useVirtualProtect = true)
    {
        var read = SharpMem.Inst.ReadFuncs.ReadByteArrayDefaultEndian;

        if(typeof(T).IsBasicType())
        {
            BasicTypeConverter converter = basicTypeConverterDict[typeof(T)];
            Memory<byte> memoryData = ReadByteArrayDefaultEndian(address.value, (uint)Marshal.SizeOf(typeof(T)), useVirtualProtect);
            return (T)converter(memoryData);
        }


        if(typeof(T) == typeof(Vector2) || typeof(T) == typeof(Vector3))
        {
            int size = (typeof(T) == typeof(Vector2)) ? sizeof(Single) * 2 : sizeof(Single) * 3;
            Memory<byte> memoryData = ReadByteArrayDefaultEndian(address.value, (uint)size, useVirtualProtect);
            Span<byte> span = memoryData.Span;

            if(typeof(T) == typeof(Vector2))
            {
                return (T)(object)new Vector2(
                    BitConverter.ToSingle(span.Slice(0, sizeof(Single))),
                    BitConverter.ToSingle(span.Slice(sizeof(Single), sizeof(Single)))
                );
            }
            else
            {
                return (T)(object)new Vector3(
                    BitConverter.ToSingle(span.Slice(0, sizeof(Single))),
                    BitConverter.ToSingle(span.Slice(sizeof(Single), sizeof(Single))),
                    BitConverter.ToSingle(span.Slice(sizeof(Single) * 2, sizeof(Single)))
                );
            }
        }

        return (T)(object)null!;
    }


    byte[] reusableBuffer;
    public Memory<byte> ReadByteArrayLittleEndian(long address, uint sizeToRead, bool useVirtualProtect = true)
    {
        if(!SharpMem.Inst.IsConnectedToProcess)
            throw new System.Exception();

        if(reusableBuffer == null || reusableBuffer.Length == 0)
            reusableBuffer = new byte[SIZE_512mb];

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

    public void ClearBuffer() => reusableBuffer = null;

    public Memory<byte> ReadByteArrayDefaultEndian(long address, uint sizeToRead, bool useVirtualProtect = true)
    {
        if(!SharpMem.Inst.IsConnectedToProcess)
            throw new System.Exception();

        if(reusableBuffer == null || reusableBuffer.Length < sizeToRead)
            reusableBuffer = new byte[sizeToRead];

        Memory<byte> memoryBuffer = new Memory<byte>(reusableBuffer);
        uint oldProtect = 0;

        if(useVirtualProtect)
            VirtualProtectEx(SharpMem.Inst.ProcessHandle, (IntPtr)address, sizeToRead, PAGE_READWRITE, out oldProtect);

        try
        {
            ReadProcessMemory(SharpMem.Inst.ProcessHandle, (IntPtr)address, reusableBuffer, sizeToRead, out uint bytesRead);
            memoryBuffer = memoryBuffer.Slice(0, (int)bytesRead);

            if(Endianness == Endianness.BigEndian)
            {
                memoryBuffer.Span.Reverse();
            }
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

    public string ReadStringAscii(long address, uint size) => Encoding.ASCII.GetString(ReadByteArrayDefaultEndian(address, size).Span);
    public string ReadStringUnicode(long address, uint size) => Encoding.Unicode.GetString(ReadByteArrayDefaultEndian(address, size).Span);
    public string ReadStringUTF8(long address, uint size) => Encoding.UTF8.GetString(ReadByteArrayDefaultEndian(address, size).Span);
}