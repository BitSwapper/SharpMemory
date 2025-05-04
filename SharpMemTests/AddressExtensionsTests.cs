using System.Diagnostics;
using System.Numerics;
using Xunit;
using static SharpMemory.Native.NativeData;

namespace SharpMemory.Tests;

[Collection("Sequential")]
public class AddressExtensionsTests : IDisposable
{
    private readonly SharpMem _sharpMem;

    public AddressExtensionsTests()
    {
        Process.Start("notepad.exe");
        _sharpMem = new SharpMem("notepad", ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite | ProcessAccessFlags.VMOperation | ProcessAccessFlags.QueryInformation);
        AddressExtensions.Initialize(_sharpMem);
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
    public void Initialize_WithNullSharpMem_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => AddressExtensions.Initialize(null!));
    }

    [Fact]
    public void WriteAndRead_BasicTypes_ShouldWork()
    {
        // Arrange
        var baseAddress = new Address(_sharpMem.Process!.MainModule!.BaseAddress.ToInt64() + 0x20000);

        // Test int
        baseAddress.Write(42);
        Assert.Equal(42, baseAddress.Read<int>());

        // Test float
        baseAddress.Write(42.5f);
        Assert.Equal(42.5f, baseAddress.Read<float>());

        // Test byte
        baseAddress.Write((byte)42);
        Assert.Equal((byte)42, baseAddress.Read<byte>());
    }

    [Fact]
    public void WriteAndRead_Vector2_ShouldWork()
    {
        // Arrange
        var baseAddress = new Address(_sharpMem.Process!.MainModule!.BaseAddress.ToInt64() + 0x20000);
        var testVector = new Vector2(1.5f, 2.5f);

        // Act
        baseAddress.Write(new Vector3(testVector.X, testVector.Y, 0));
        var result = baseAddress.Read<Vector2>();

        // Assert
        Assert.Equal(testVector.X, result.X);
        Assert.Equal(testVector.Y, result.Y);
    }

    [Fact]
    public void WriteAndRead_Vector3_ShouldWork()
    {
        // Arrange
        var baseAddress = new Address(_sharpMem.Process!.MainModule!.BaseAddress.ToInt64() + 0x20000);
        var testVector = new Vector3(1.5f, 2.5f, 3.5f);

        // Act
        baseAddress.Write(testVector);
        var result = baseAddress.Read<Vector3>();

        // Assert
        Assert.Equal(testVector.X, result.X);
        Assert.Equal(testVector.Y, result.Y);
        Assert.Equal(testVector.Z, result.Z);
    }

    [Fact]
    public void ReadString_DifferentEncodings_ShouldWork()
    {
        // Arrange
        var baseAddress = _sharpMem.Process!.MainModule!.BaseAddress.ToInt64() + 0x20000;
        var testString = "Test String";

        // ASCII
        var asciiResult = AddressExtensions.ReadStringAscii(baseAddress, (uint)testString.Length);
        Assert.NotNull(asciiResult);

        // Unicode
        var unicodeResult = AddressExtensions.ReadStringUnicode(baseAddress, (uint)testString.Length);
        Assert.NotNull(unicodeResult);

        // UTF8
        var utf8Result = AddressExtensions.ReadStringUTF8(baseAddress, (uint)testString.Length);
        Assert.NotNull(utf8Result);
    }

    [Fact]
    public void WriteAndRead_WithVirtualProtect_ShouldWork()
    {
        // Arrange
        var baseAddress = new Address(_sharpMem.Process!.MainModule!.BaseAddress.ToInt64() + 0x20000);
        var testValue = 42;

        // Act & Assert
        Assert.True(baseAddress.Write(testValue, useVirtualProtect: true));
        Assert.Equal(testValue, baseAddress.Read<int>(useVirtualProtect: true));
    }

    [Fact]
    public void VectorOperations_WithOffset_ShouldWork()
    {
        // Arrange
        var baseAddress = new Address(_sharpMem.Process!.MainModule!.BaseAddress.ToInt64() + 0x20000);
        var testVector = new Vector3(1.0f, 2.0f, 3.0f);

        // Act
        baseAddress.Write(testVector);

        // Assert individual components with offsets
        Assert.Equal(testVector.X, (baseAddress + 0).Read<float>());
        Assert.Equal(testVector.Y, (baseAddress + 4).Read<float>());
        Assert.Equal(testVector.Z, (baseAddress + 8).Read<float>());
    }
}

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection { }