namespace SharpMemory;
internal class TypeHelper
{
    public static bool IsBasicType(Type type)
    {
        if(type == typeof(Byte)    ||
           type == typeof(Char)    ||
           type == typeof(Boolean) ||
           type == typeof(Int16)   ||
           type == typeof(Int32)   ||
           type == typeof(Int64)   ||
           type == typeof(UInt16)  ||
           type == typeof(UInt32)  ||
           type == typeof(UInt64)  ||
           type == typeof(Single)  ||
           type == typeof(Double))
            return true;
        return false;
    }
}
