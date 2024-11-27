using System.Diagnostics;
using System.Numerics;
namespace SharpMemory;
public class PatternScanning
{
    const int SIZE_80kb = 80 * 1024;
    SharpMem SharpMem { get; init; }

    public PatternScanning(SharpMem sharpMem)
    {
        SharpMem = sharpMem;
    }

    public Address FindPatternInDynaMappedModule(string pattern, int chunkSize = SIZE_80kb, string endsWith = "")//may want endsWith 0000 for example for 007 AuF; any emulated mirror base will end in 4 zeros
    {
        var modules = SharpMem.ModuleFuncs.GetAllModules();
        var dynaMappedModules = SharpMem.MemoryAnalyzer.FindUnknownMemory(modules, SharpMem.ProcessHandle);

        List<long> possibleMatches = new();
        foreach(var module in dynaMappedModules)
        {
            long moduleBaseAddress = module.Key;
            long moduleEndAddress = module.Value;
            uint moduleSize = (uint)(moduleEndAddress - moduleBaseAddress);//lord have mercy on me if a module is bigger than uint. too lazy to fix now :Skull:
            long currentModulePositionOffset = 0;

            foreach(var chunk in SharpMem.ReadFuncs.ReadByteArrayChunkedLittleEndian(moduleBaseAddress, moduleSize, chunkSize, false))
            {
                long resultStr = SharpMem.PatternScanning.PatternScanManual(pattern, chunk);
                if(resultStr != -1)
                {
                    possibleMatches.Add(moduleBaseAddress + currentModulePositionOffset + resultStr);
                    break;
                }
                currentModulePositionOffset += chunk.Length;
            }
        }


        if(endsWith != "")
            return possibleMatches.Where(m => m.ToString("X").EndsWith(endsWith)).First();

        return possibleMatches.First();
    }


    public static long PatternScan(byte[] patternBytes, Memory<byte> dumpBytes)
    {
        Span<byte> dumpSpan = dumpBytes.Span;
        int patternLength = patternBytes.Length;
        int dumpLength = dumpSpan.Length;

        if(patternLength == 0 || dumpLength < patternLength)
        {
            return -1;
        }

        int lastPatternByte = patternLength - 1;
        byte lastPatternByteValue = patternBytes[lastPatternByte];
        int lastIndex = dumpLength - patternLength;

        for(int i = 0; i <= lastIndex; i++)
        {
            //First, check the last byte to quickly skip non-matching sections
            if(dumpSpan[i + lastPatternByte] == lastPatternByteValue || lastPatternByteValue == 0xFF)
            {
                //Then, compare the entire pattern starting from the last match
                bool found = true;
                for(int j = lastPatternByte - 1; j >= 0; j--)
                {
                    byte patternByte = patternBytes[j];
                    if(patternByte != 0xFF && patternByte != dumpSpan[i + j])
                    {
                        found = false;
                        break;
                    }
                }

                if(found)
                {
                    return i;
                }
            }
        }

        return -1;
    }


    public long PatternScanManual(string pattern, Memory<byte> memDump)
    {
        byte[] patternBytes = ConvertHexStringToByteArray(pattern);
        long result = PatternScan(patternBytes, memDump);

        patternBytes = null;
        memDump = null;
        return result;
    }

    public bool PatternScanModule(ProcessModule module, string pattern, out long patternAddress, bool useVirtualProtect = false)
    {
        patternAddress = -1;
        long baseAddress = (long)module.BaseAddress;
        uint size = (uint)module.ModuleMemorySize;
        Memory<byte> memDump = SharpMem.ReadFuncs.ReadByteArrayDefaultEndian(baseAddress, size, useVirtualProtect);

        byte[] patternBytes = GetPatternBytes(pattern);

        long result = PatternScan(patternBytes, memDump);

        if(result != -1)
        {
            patternAddress = baseAddress + result;
            return true;
        }

        return false;
    }

    byte[] GetPatternBytes(string pattern)
    {
        string[] hexStrings = pattern.Split(' ');
        byte[] bytes = new byte[hexStrings.Length];

        for(int i = 0; i < hexStrings.Length; i++)
        {
            if(hexStrings[i].Contains("?"))
                bytes[i] = 0xFF;
            else
                bytes[i] = Convert.ToByte(hexStrings[i], 16);
        }

        return bytes;
    }

    byte[] ConvertHexStringToByteArray(string hexString)
    {
        hexString = hexString.Replace(" ", "");
        hexString = PadWithZeroIfOddLength(hexString);

        int length = hexString.Length;
        List<byte> bytes = new();

        for(int i = 0; i < length; i += 2)
        {
            string byteString = hexString.Substring(i, 2);

            if(byteString == "??")//wildcard
                bytes.Add(0xFF);
            else
                bytes.Add(byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber));
        }

        return bytes.ToArray();

        static string PadWithZeroIfOddLength(string hexString)
        {
            if(hexString.Length % 2 != 0)
                hexString = "0" + hexString;
            return hexString;
        }
    }
}



//public long PatternScanOld(byte[] patternBytes, Memory<byte> dumpBytes)
//{
//    Span<byte> dumpSpan = dumpBytes.Span;
//    int patternLength = patternBytes.Length;
//    int dumpLength = dumpSpan.Length;

//    if(dumpLength >= patternLength)
//    {
//        for(int i = 0; i <= dumpLength - patternLength; i++)
//        {
//            bool found = true;

//            for(int j = 0; j < patternLength; j++)
//            {
//                if(patternBytes[j] != 0xFF && patternBytes[j] != dumpSpan[(int)(i + j)])
//                {
//                    found = false;
//                    break;
//                }
//            }

//            if(found)
//                return i;
//        }
//    }

//    return -1;
//}