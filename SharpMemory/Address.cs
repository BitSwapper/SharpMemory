namespace SharpMemory;
public class Address
{
    public long value { get; set; }
    public Address(long value) => this.value = value;
    public Address(Address address) => this.value = address.value;

    public static implicit operator Address(long offset) => new Address(offset);
    public static Address operator +(Address a, Address b) => new Address(a.value + b.value);
    public static Address operator -(Address a, Address b) => new Address(a.value - b.value);
    public override string ToString() => value.ToString();
}