using System.Numerics;

namespace pkuManager.Formats.Fields;

public interface IIntegralField : IField<BigInteger>
{
    public abstract BigInteger? Max { get; }
    public abstract BigInteger? Min { get; }
}