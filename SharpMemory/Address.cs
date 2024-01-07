public class Address
{
    public long value { get; set; }

    public Address(long value) => this.value = value;

    public Address(Address address) => value = address.value;

    // long to Address
    public static implicit operator Address(long offset) => new Address(offset);

    //int to long to Address*
    public static implicit operator Address(int offset) => new((long)offset);

    //uint to long to Address*
    public static implicit operator Address(uint offset) => new((long)offset);

    //IntPtr to Address
    public static implicit operator Address(IntPtr offset) => new Address((long)offset);

    //Address to long 
    public static implicit operator long(Address address) => address.value;


    public static Address operator +(Address a, long b) => new Address(a.value + b);

    public static Address operator +(Address a, int b) => new Address(a.value + b);

    public static Address operator +(Address a, Address b) => new Address(a.value + b.value);

    public static Address operator -(Address a, long b) => new Address(a.value - b);

    public static Address operator -(Address a, int b) => new Address(a.value - b);

    public static Address operator -(Address a, Address b) => new Address(a.value - b.value);

    public override bool Equals(object obj)
    {
        if(obj is Address otherAddress)
        {
            return value.Equals(otherAddress.value);
        }
        return false;
    }

    public static bool operator ==(Address left, Address right)
    {
        if(ReferenceEquals(left, right))
            return true;

        if(left is null || right is null)
            return false;

        return left.value == right.value;
    }

    public static bool operator !=(Address left, Address right) => !(left == right);

    public override string ToString() => value.ToString();

    public string ToStringHex() => value.ToString("X");


    public override int GetHashCode() => value.GetHashCode();
}
