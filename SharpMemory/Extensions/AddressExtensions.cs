using System.Numerics;

namespace SharpMemory;

public static class AddressExtensions
{
    public static bool Write<T>(this Address address, T value, bool useVirtualProtect = true) => SharpMem.Inst.WriteFuncs.Write<T>(address, value, useVirtualProtect);
    public static T Read<T>(this Address address, bool useVirtualProtect = true) => SharpMem.Inst.ReadFuncs.Read<T>(address, useVirtualProtect);

    public static void WriteVector3(this Address address, Vector3 value, bool useVirtualProtect = true)
    {
        address.Write<float>(value.X, useVirtualProtect);
        (address + 4).Write<float>(value.Y, useVirtualProtect);
        (address + 8).Write<float>(value.Z, useVirtualProtect);
    }

    public static Vector3 ReadVector3(this Address address, bool useVirtualProtect = true)
    {
        float X = address.Read<float>(useVirtualProtect);
        float Y = (address + 4).Read<float>(useVirtualProtect);
        float Z = (address + 8).Read<float>(useVirtualProtect);
        return new Vector3(X, Y, Z);
    }
}