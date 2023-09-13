using System.Numerics;

namespace SharpMemory;

public static class AddressExtensions
{
    public static bool Write<T>(this Address address, T value, bool useVirtualProtect = true) => SharpMem.Inst.WriteFuncs.Write<T>(address, value, useVirtualProtect);
    public static T Read<T>(this Address address) => SharpMem.Inst.ReadFuncs.Read<T>(address);

    public static void WriteVector3(this Address address, Vector3 value)
    {
        address.Write<float>(value.X);
        (address + 4).Write<float>(value.Y);
        (address + 8).Write<float>(value.Z);
    }

    public static Vector3 ReadVector3(this Address address)
    {
        float X = address.Read<float>();
        float Y = (address + 4).Read<float>();
        float Z = (address + 8).Read<float>();
        return new Vector3(X, Y, Z);
    }
}