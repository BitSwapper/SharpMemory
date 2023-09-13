using System.Diagnostics;

namespace SharpMemory;
public class PatternScanning
{
    public bool PatternScanModule(ProcessModule module, string pattern, out long patternAddress)
    {
        patternAddress = -1;
        long baseAddress = (long)module.BaseAddress;
        uint size = (uint)module.ModuleMemorySize;
        byte[] memDump = SharpMem.Inst.ReadFuncs.ReadByteArray(baseAddress, size);

        // Split the pattern string into an array of bytes
        byte[] patternBytes = GetPatternBytes(pattern);

        // Iterate through the memory dump and try to find a match for the pattern
        for(int i = 0; i < memDump.Length - patternBytes.Length; i++)
        {
            bool match = true;
            for(int j = 0; j < patternBytes.Length; j++)
            {
                if(patternBytes[j] != memDump[i + j] && patternBytes[j] != 255)
                {
                    match = false;
                    break;
                }
            }
            if(match)
            {
                patternAddress = baseAddress + i;
                return true;
            }
        }
        return false;
    }

    private byte[] GetPatternBytes(string pattern)
    {
        // Split the pattern string into an array of strings
        string[] hexStrings = pattern.Split(' ');
        byte[] bytes = new byte[hexStrings.Length];

        // Convert each string to a byte
        for(int i = 0; i < hexStrings.Length; i++)
        {
            if(hexStrings[i].Contains("?"))// Use a placeholder value of 255 to indicate that any byte will match -------This is a problem lowkey lol
                bytes[i] = 255;
            else
                bytes[i] = Convert.ToByte(hexStrings[i], 16);
        }

        return bytes;
    }
}
