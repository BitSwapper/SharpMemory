using Xunit;
using SharpMemory.Enums;
using System.Diagnostics;
using static SharpMemory.Native.NativeData;
using Xunit.Sdk;

namespace SharpMemory.Tests;

public class SharpMemTests : IDisposable
{
    private Process? _testProcess;

    public SharpMemTests()
    {
        StartTestProcess();
    }

    public void Dispose()
    {
        if(_testProcess is not null && !_testProcess.HasExited)
        {
            try { _testProcess.Kill(); }
            catch { /* Ignore cleanup errors */ }
        }
        _testProcess?.Dispose();
    }

    [Theory]
    [InlineData("notepad.exe")]
    [InlineData("notepad")]
    public void Constructor_WithValidProcessName_ShouldInitializeCorrectly(string processName)
    {
        // Act
        using var sharpMem = new SharpMem(
            processName,
            ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite
        );

        // Assert
        Assert.True(sharpMem.IsConnectedToProcess);
        Assert.NotNull(sharpMem.Process);
        Assert.NotEqual(IntPtr.Zero, sharpMem.ProcessHandle);
        Assert.NotNull(sharpMem.ReadFuncs);
        Assert.NotNull(sharpMem.WriteFuncs);
    }

    [Fact]
    public void Constructor_WithInvalidProcessName_ShouldThrowException()
    {
        // Arrange
        const string invalidProcessName = "ThisProcessDoesNotExist123";

        // Act & Assert
        var exception = Assert.Throws<ProcessNotFoundException>(() =>
            new SharpMem(invalidProcessName, ProcessAccessFlags.VMRead)
        );
        Assert.Contains(invalidProcessName, exception.Message);
    }

    [Fact]
    public void Dispose_ShouldCleanupResources()
    {
        // Arrange
        var sharpMem = new SharpMem(
            "notepad",
            ProcessAccessFlags.VMRead
        );
        var initialHandle = sharpMem.ProcessHandle;

        // Act
        sharpMem.Dispose();

        // Assert
        Assert.Equal(IntPtr.Zero, sharpMem.ProcessHandle);
        Assert.Null(sharpMem.ReadFuncs);
        Assert.Null(sharpMem.WriteFuncs);
    }

    [Fact]
    public void SetEndianness_ShouldUpdateReadAndWriteFunctions()
    {
        // Arrange
        using var sharpMem = new SharpMem(
            "notepad",
            ProcessAccessFlags.VMRead
        );
        var initialReadFuncs = sharpMem.ReadFuncs;
        var initialWriteFuncs = sharpMem.WriteFuncs;

        // Act
        sharpMem.SetEndianness(Endianness.BigEndian);

        // Assert
        Assert.NotSame(initialReadFuncs, sharpMem.ReadFuncs);
        Assert.NotSame(initialWriteFuncs, sharpMem.WriteFuncs);
    }

    [Fact]
    public void Constructor_WithInvalidFlags_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ProcessAccessException>(() =>
            new SharpMem("notepad", 0) // Invalid flags
        );
        Assert.Contains("Failed to get required access", exception.Message);
    }

    void StartTestProcess()
    {
        try
        {
            _testProcess = Process.Start("notepad.exe");
        }
        catch
        {
            throw new Exception("Cannot start notepad.exe for testing");
        }
    }
}
