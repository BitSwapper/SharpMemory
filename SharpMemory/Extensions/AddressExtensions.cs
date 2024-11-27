using System.Numerics;
using System.Text;

namespace SharpMemory;

public static class AddressExtensions
{
    static SharpMem SharpMem { get; set; }
    public static void Initialize(SharpMem sharpMem) => SharpMem = sharpMem ?? throw new ArgumentNullException(nameof(sharpMem));

    public static bool Write<T>(this Address address, T value, bool useVirtualProtect = false) => SharpMem.WriteFuncs.Write<T>(address, value, useVirtualProtect);
    public static T Read<T>(this Address address, bool useVirtualProtect = false) => SharpMem.ReadFuncs.Read<T>(address, useVirtualProtect);

    public static void WriteVector2(this Address address, Vector3 value, bool useVirtualProtect = false)
    {
        address.Write<float>(value.X, useVirtualProtect);
        (address + 4).Write<float>(value.Y, useVirtualProtect);
    }

    public static void WriteVector3(this Address address, Vector3 value, bool useVirtualProtect = false)
    {
        address.Write<float>(value.X, useVirtualProtect);
        (address + 4).Write<float>(value.Y, useVirtualProtect);
        (address + 8).Write<float>(value.Z, useVirtualProtect);
    }

    public static Vector2 ReadVector2(this Address address, bool useVirtualProtect = false)
    {
        float X = address.Read<float>(useVirtualProtect);
        float Y = (address + 4).Read<float>(useVirtualProtect);
        return new Vector2(X, Y);
    }

    public static Vector3 ReadVector3(this Address address, bool useVirtualProtect = false)
    {
        float X = address.Read<float>(useVirtualProtect);
        float Y = (address + 4).Read<float>(useVirtualProtect);
        float Z = (address + 8).Read<float>(useVirtualProtect);
        return new Vector3(X, Y, Z);
    }

    public static string ReadStringAscii(long address, uint size, bool useVirtualProtect = false) => SharpMem.ReadFuncs.ReadStringAscii(address, size, useVirtualProtect);
    public static string ReadStringUnicode(long address, uint size, bool useVirtualProtect = false) => SharpMem.ReadFuncs.ReadStringUnicode(address, size, useVirtualProtect);
    public static string ReadStringUTF8(long address, uint size, bool useVirtualProtect = false) => SharpMem.ReadFuncs.ReadStringUTF8(address, size, useVirtualProtect);
}