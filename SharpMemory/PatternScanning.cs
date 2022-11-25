using System.Diagnostics;
using System.Globalization;

namespace SharpMemory;
public class PatternScanning
{
    private bool CheckPatternMatches(string pattern, byte[] array2check)
    {
        int length = array2check.Length;
        string[] patternBytes = pattern.Split(' ');
        int matchCount = 0;

        foreach(byte currentByte in array2check)
        {
            if(patternBytes[matchCount].Equals("?") || patternBytes[matchCount].Equals("??"))
                matchCount++;
            else if(byte.Parse(patternBytes[matchCount], NumberStyles.HexNumber) == currentByte)
                matchCount++;
            else
                return false;
        }
        return true;
    }

    public bool PatternScanModule(ProcessModule module, string pattern, out long patternAddress)
    {
        patternAddress = -1;
        long baseAddress = (long)module.BaseAddress;
        uint size = (uint)module.ModuleMemorySize;
        byte[] memDump = SharpMem.Inst.ReadFuncs.ReadByteArray(baseAddress, size);
        string[] patternBytes = pattern.Split(' ');
        try
        {
            for(int dumpIndex = 0; dumpIndex < memDump.Length; dumpIndex++)
            {
                if(memDump[dumpIndex] == byte.Parse(patternBytes[0], NumberStyles.HexNumber))
                {
                    byte[] checkArray = new byte[patternBytes.Length];
                    for(int patternIndex = 0; patternIndex < patternBytes.Length; patternIndex++)
                        checkArray[patternIndex] = memDump[dumpIndex + patternIndex];

                    if(CheckPatternMatches(pattern, checkArray))
                        patternAddress = (long)baseAddress + dumpIndex;
                    else
                        dumpIndex += 1;
                }
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
}
