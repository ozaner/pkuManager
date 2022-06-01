using System.Numerics;

namespace pkuManager.Formats.Fields;

public interface IBoundable
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }
}