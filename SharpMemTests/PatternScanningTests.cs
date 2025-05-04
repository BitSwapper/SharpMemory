using System.Diagnostics;
using static SharpMemory.Native.NativeData;

namespace SharpMemory.Tests;

[Collection("Sequential")]
public class PatternScanningTests : IDisposable
{
    private readonly SharpMem _sharpMem;
    private readonly PatternScanning _patternScanning;

    public PatternScanningTests()
    {
        Process.Start("notepad.exe");
        _sharpMem = new SharpMem("notepad", ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite | ProcessAccessFlags.VMOperation | ProcessAccessFlags.QueryInformation);
        _patternScanning = new PatternScanning(_sharpMem);
    }

    public void Dispose()
    {
        _sharpMem.Dispose();
        foreach(var proc in Process.GetProcessesByName("notepad"))
        {
            try { proc.Kill(); } catch { }
        }
    }

    [Fact]
    public void PatternScan_BasicPattern_ShouldFind()
    {
        // Arrange
        byte[] pattern = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in hex
        byte[] data = new byte[] { 0x00, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00 };

        // Act
        long result = PatternScanning.PatternScan(pattern, data);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void PatternScan_WithWildcard_ShouldFind()
    {
        // Arrange
        byte[] pattern = new byte[] { 0x48, 0xFF, 0x6C, 0xFF, 0x6F }; // "H?l?o" with wildcards
        byte[] data = new byte[] { 0x48, 0x00, 0x6C, 0x00, 0x6F };

        // Act
        long result = PatternScanning.PatternScan(pattern, data);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void PatternScan_NoMatch_ShouldReturnMinusOne()
    {
        // Arrange
        byte[] pattern = new byte[] { 0x48, 0x65, 0x6C };
        byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        // Act
        long result = PatternScanning.PatternScan(pattern, data);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void PatternScanManual_ValidPattern_ShouldFind()
    {
        // Arrange
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };

        // Act
        long result = _patternScanning.PatternScanManual("48 65 6C 6C 6F", data);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void PatternScanManual_WithWildcard_ShouldFind()
    {
        // Arrange
        var data = new byte[] { 0x48, 0x00, 0x6C, 0x00, 0x6F };

        // Act
        long result = _patternScanning.PatternScanManual("48 ?? 6C ?? 6F", data);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetPatternBytes_ValidPattern_ShouldConvert()
    {
        // Arrange
        string pattern = "48 65 ? 6C 6F";

        // Act
        byte[] result = _patternScanning.GetPatternBytes(pattern);

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Equal(0x48, result[0]);
        Assert.Equal(0x65, result[1]);
        Assert.Equal(0xFF, result[2]); // wildcard
        Assert.Equal(0x6C, result[3]);
        Assert.Equal(0x6F, result[4]);
    }

    [Fact]
    public void ConvertHexStringToByteArray_ValidHexString_ShouldConvert()
    {
        // Arrange
        string hexString = "48 65 ?? 6C 6F";

        // Act
        byte[] result = _patternScanning.ConvertHexStringToByteArray(hexString);

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Equal(0x48, result[0]);
        Assert.Equal(0x65, result[1]);
        Assert.Equal(0xFF, result[2]); // wildcard
        Assert.Equal(0x6C, result[3]);
        Assert.Equal(0x6F, result[4]);
    }

    [Fact]
    public void PatternScanModule_ShouldFindPattern()
    {
        // Arrange
        var module = _sharpMem.Process!.MainModule!;
        var pattern = "4D 5A 90"; // DOS header magic numbers

        // Act
        bool found = _patternScanning.PatternScanModule(module, pattern, out Address patternAddress);

        // Assert
        Assert.True(found);
        Assert.Equal(module.BaseAddress, patternAddress); // Should be at the start of the module
    }

    [Theory]
    [InlineData("ZZ")] // Invalid hex
    [InlineData("48 ZZ")] // Invalid hex with space
    public void ConvertHexStringToByteArray_InvalidInput_ShouldThrowFormatException(string invalidHex)
    {
        Assert.Throws<FormatException>(() =>
            _patternScanning.ConvertHexStringToByteArray(invalidHex));
    }
}