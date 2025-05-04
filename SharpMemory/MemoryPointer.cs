using System.Numerics;
using SharpMemory.Enums;

namespace SharpMemory;

public class MemoryPointer<T>
{
    public Address AddressBeingPointedTo => DetermineBaseAddressOfPointerChain();
    public Address BaseAddress { get; private init; }
    public int[] PointerOffsets { get; private init; }
    public int PositionalOffset { get; private init; }
    BitType bitType { get; init; }

    public MemoryPointer(Address address, int[] pointerOffsets, int positionalOffset = 0, BitType bitType = BitType.x64)
    {
        BaseAddress = address;
        PositionalOffset = positionalOffset;
        PointerOffsets = pointerOffsets;
        this.bitType = bitType;
    }

    public T Read()
    {
        Address currentAddress = AddressBeingPointedTo;

        if(currentAddress == 0)
            return default;

        //if(typeof(T) == typeof(Vector3))
        //    return (T)(object)currentAddress.ReadVector3();
        //else
            return currentAddress.Read<T>();
    }

    public void Write(T value)
    {
        Address currentAddress = AddressBeingPointedTo;
        if(currentAddress == 0) return;

       //if(typeof(T) == typeof(Vector3))
       //    currentAddress.WriteVector3((Vector3)(object)value!);
       //else
            currentAddress.Write(value);
    }

    Address DetermineBaseAddressOfPointerChain()
    {
        Address currentAddress = BaseAddress;

        foreach(int offset in PointerOffsets)
        {
            if(bitType == BitType.x86)
                currentAddress = currentAddress.Read<int>();
            else
                currentAddress = currentAddress.Read<long>();

            if(currentAddress == 0)
                return 0;

            currentAddress += offset;
        }

        return currentAddress + PositionalOffset;
    }
}