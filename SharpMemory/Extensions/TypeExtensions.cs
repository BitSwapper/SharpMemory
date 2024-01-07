namespace SharpMemory;

public static class TypeExtensions
{
    static HashSet<Type> basicTypes = new()
    {
        typeof(byte),
        typeof(char),
        typeof(bool),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(float),
        typeof(double)
    };

    public static bool IsBasicType(this Type type) => basicTypes.Contains(type);
}
