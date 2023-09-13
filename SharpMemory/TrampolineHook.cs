using System.Runtime.InteropServices;
using static SharpMemory.Native.NativeData;

namespace SharpMemory;

public class TrampolineHook
{
    public void Place(Address targetFunctionAddress, int targetFunctionPrologueLength, byte[] newFunction, Address overrideAddy)
    {
        byte[] targetFunctionPrologue = new byte[targetFunctionPrologueLength];
        targetFunctionPrologue = SharpMem.Inst.ReadFuncs.ReadByteArray(targetFunctionAddress.value, (uint)targetFunctionPrologue.Length);
        IntPtr allocatedMemory = SharpMem.Inst.MemoryAllocator.AllocateMemory(SharpMem.Inst.ProcessHandle, (uint)newFunction.Length, (IntPtr)overrideAddy.value);

        long allocatedMemoryValue = (long)allocatedMemory;
        // Write our own code to the allocated memory
        SharpMem.Inst.WriteFuncs.WriteByteArray(allocatedMemoryValue, newFunction);

        int jumpSize = 5;

        byte[] nops = new byte[targetFunctionPrologueLength - jumpSize];
        for(int i = 0; i < nops.Length; i++)
            nops[i] = 0x90;


        long jumpAddress = (long)allocatedMemory;//((allocatedMemory.ToInt64() - targetFunctionAddress.value) - targetFunctionPrologueLength);
        byte[] bytes = BitConverter.GetBytes(jumpAddress);

        if(!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        byte[] jumpInstruction = new byte[] { 0xE9 }.Concat(bytes).ToArray();
        byte[] fullInstruction = jumpInstruction.Concat(nops).ToArray();
        SharpMem.Inst.WriteFuncs.WriteByteArray(targetFunctionAddress.value, fullInstruction);
    }
}
