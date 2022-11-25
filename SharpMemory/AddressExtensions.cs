namespace SharpMemory;
public static class AddressExtensions
{
    public static bool Write<T>(this Address address, T value, bool useVirtualProtect = true) => SharpMem.Inst.WriteFuncs.Write<T>(address, value, useVirtualProtect);
    public static T Read<T>(this Address address) => SharpMem.Inst.ReadFuncs.Read<T>(address);
}