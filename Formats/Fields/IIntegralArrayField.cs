using System.Numerics;

namespace pkuManager.Formats.Fields;

public interface IIntegralArrayField : IArrayField<BigInteger>
{
    public abstract BigInteger? Max { get; }
    public abstract BigInteger? Min { get; }
}