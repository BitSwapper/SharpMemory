namespace SharpMemory;

public static class TypeExtensions
{
    public static bool IsBasicType(this Type type)
    {
        if (type == typeof(byte) ||
           type == typeof(char) ||
           type == typeof(bool) ||
           type == typeof(short) ||
           type == typeof(int) ||
           type == typeof(long) ||
           type == typeof(ushort) ||
           type == typeof(uint) ||
           type == typeof(ulong) ||
           type == typeof(float) ||
           type == typeof(double))
            return true;
        return false;
    }
}
