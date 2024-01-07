using System.Diagnostics;
using System.Text;

namespace SharpMemory;
public class PatternScanning
{
    public Address FindPatternInDynaMappedModule(string pattern, string endsWith = "")//endsWith 0000 for example for 007 AuF; any emulated mirror will end in 4 zeros
    {
        var modules = SharpMem.Inst.ModuleFuncs.GetAllModules();
        var dynaMappedModules = SharpMem.Inst.MemoryAnalyzer.FindUnknownMemory(modules, SharpMem.Inst.ProcessHandle);

        List<long> possibleMatches = new();
        foreach(var module in dynaMappedModules)
        {
            long moduleBaseAddress = module.Key;
            long moduleEndAddress = module.Value;
            uint moduleSize = (uint)(moduleEndAddress - moduleBaseAddress);//lord have mercy on me if a module is bigger than uint. too lazy to fix now :Skull:
            Memory<byte> dumpBytes = SharpMem.Inst.ReadFuncs.ReadByteArrayLittleEndian(moduleBaseAddress, moduleSize, false);

            long resultStr = SharpMem.Inst.PatternScanning.PatternScanManual(pattern, dumpBytes);
            if(resultStr != -1)
                possibleMatches.Add(moduleBaseAddress + resultStr);
        }

        if(endsWith != "")
            return possibleMatches.Where(m => m.ToString("X").EndsWith(endsWith)).First();

        return possibleMatches.First();
    }

    public long PatternScan(byte[] patternBytes, Memory<byte> dumpBytes)
    {
        Span<byte> dumpSpan = dumpBytes.Span;

        for(long i = 0; i < dumpSpan.Length - patternBytes.Length; i++)
        {
            bool found = true;

            for(int j = 0; j < patternBytes.Length; j++)
            {
                if(patternBytes[j] != 0xFF && patternBytes[j] != dumpSpan[(int)(i + j)])
                {
                    found = false;
                    break;
                }
            }

            if(found)
                return i;
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
        Memory<byte> memDump = SharpMem.Inst.ReadFuncs.ReadByteArrayDefaultEndian(baseAddress, size, useVirtualProtect);

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

        if(hexString.Length % 2 != 0)// Pad with a leading zero if the length is odd
            hexString = "0" + hexString;

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
    }
}
