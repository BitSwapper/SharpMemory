public class Address
{
    public long value { get; set; }

    public Address(long value) => this.value = value;

    public Address(Address address) => value = address.value;

    public static implicit operator Address(long offset) => new Address(offset);
    public static implicit operator Address(int offset) => new Address((long)offset);
    public static implicit operator Address(uint offset) => new Address((long)offset);
    public static implicit operator Address(IntPtr offset) => new Address((long)offset);
    public static implicit operator long(Address address) => address?.value ?? 0;

    //Explicit operators
    public static explicit operator IntPtr(Address address) => new IntPtr(address?.value ?? 0);
    public static explicit operator int(Address address) => (int)(address?.value ?? 0);
    public static explicit operator uint(Address address) => (uint)(address?.value ?? 0);

    //Addition operators
    //public static Address operator +(Address a, long b) => new Address(a?.value ?? 0 + b);
    //public static Address operator +(Address a, int b) => new Address(a?.value ?? 0 + b);
    public static Address operator +(Address a, Address b) => new Address((a?.value ?? 0) + (b?.value ?? 0));

    //Subtraction operators
    //public static Address operator -(Address a, long b) => new Address(a?.value ?? 0 - b);
    //public static Address operator -(Address a, int b) => new Address(a?.value ?? 0 - b);
    public static Address operator -(Address a, Address b) => new Address((a?.value ?? 0) - (b?.value ?? 0));

    //Multiplication operators
    public static Address operator *(Address a, Address b) => new Address((a?.value ?? 0) * (b?.value ?? 0));

    //Division operators
    public static Address operator /(Address a, Address b) => new Address((a?.value ?? 0) / (b?.value ?? 0));

    //Modulus operators
    public static Address operator %(Address a, Address b) => new Address((a?.value ?? 0) % (b?.value ?? 0));

    //Bitwise operators
    public static Address operator &(Address a, Address b) => new Address((a?.value ?? 0) & (b?.value ?? 0));

    public static Address operator |(Address a, Address b) => new Address((a?.value ?? 0) | (b?.value ?? 0));

    public static Address operator ^(Address a, Address b) => new Address((a?.value ?? 0) ^ (b?.value ?? 0));

    //Shift operators
    public static Address operator <<(Address a, int b) => new Address(a?.value ?? 0 << b);
    public static Address operator >>(Address a, int b) => new Address(a?.value ?? 0 >> b);

    //Equality operators
    public static bool operator ==(Address a, Address b) => (a is null) ? (b is null) : a?.value == b?.value;
    public static bool operator !=(Address a, Address b) => a?.value != b?.value;

    //Inequality operators
    public static bool operator <(Address a, Address b) => a?.value < b?.value;
    public static bool operator >(Address a, Address b) => a?.value > b?.value;
    public static bool operator <=(Address a, Address b) => a?.value <= b?.value;
    public static bool operator >=(Address a, Address b) => a?.value >= b?.value;

    //public bool Equals(Address other) => value.Equals(other.value); //IDK which 
    public override bool Equals(object obj)
    {
        if(obj is Address otherAddress)
            return value.Equals(otherAddress.value);
        return false;
    }

    public override string ToString() => value.ToString();

    public string ToString(string format)
    {
        if(string.IsNullOrEmpty(format)) return ToString();

        return value.ToString(format);
    }

    public override int GetHashCode() => value.GetHashCode();
}


//public static bool operator ==(Address left, Address right)
//{
//   if(ReferenceEquals(left, right))
//       return true;

//   if(left is null || right is null)
//       return false;

//   return left.value == right.value;
//}

//public static bool operator !=(Address left, Address right) => !(left == right);