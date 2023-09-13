using System.Numerics;
using SharpMemory;

namespace HackingUtilities;

public class MemoryPointer<T>
{
    public Address BaseAddress { get; set; }
    public Address AddressBeingPointedTo => DetermineBaseAddressOfPointerChain();
    public int[] PointerOffsets { get; set; }
    public int PositionalOffset { get; set; }

    public MemoryPointer(Address address, int[] pointerOffsets, int positionalOffset = 0)
    {
        BaseAddress = address;
        PositionalOffset = positionalOffset;
        PointerOffsets = pointerOffsets;
    }

    public T Read()
    {
        Address currentAddress = AddressBeingPointedTo;

        if(typeof(T) == typeof(Vector3))
            return (T)(object)currentAddress.ReadVector3();
        else
            return currentAddress.Read<T>();
    }

    public void Write(T value)
    {
        Address currentAddress = AddressBeingPointedTo;

        if(typeof(T) == typeof(Vector3))
            currentAddress.WriteVector3((Vector3)(object)value!);
        else
            currentAddress.Write(value);
    }

    Address DetermineBaseAddressOfPointerChain()
    {
        Address currentAddress = BaseAddress;

        foreach(int offset in PointerOffsets)
            currentAddress = currentAddress.Read<long>() + offset;

        return currentAddress + PositionalOffset;
    }
}