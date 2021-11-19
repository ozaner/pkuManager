namespace pkuManager.Utilities;

/// <summary>
/// Represents the discriminated union of two types, dubbed left and right.
/// </summary>
/// <typeparam name="T1">The left type.</typeparam>
/// <typeparam name="T2">The right type.</typeparam>
public struct Union<T1, T2>
{
    public T1 Left { get; }
    public T2 Right { get; }

    public bool IsLeft => !IsRight;
    public bool IsRight { get; }

    public Union(T1 left) => (Left, Right, IsRight) = (left, default, false);
    public Union(T2 right) => (Left, Right, IsRight) = (default, right, true);

    public static implicit operator Union<T1, T2>(T1 left) => new(left);
    public static implicit operator Union<T1, T2>(T2 right) => new(right);

    public static bool operator ==(Union<T1, T2> u1, Union<T1, T2> u2)
        => u1.IsRight && u2.IsRight && u1.Right.Equals(u2.Right) ||
           u1.IsLeft && u2.IsLeft && u1.Left.Equals(u2.Left);

    public static bool operator !=(Union<T1, T2> u1, Union<T1, T2> u2) => !(u1 == u2);

    public override bool Equals(object obj) => this == (Union<T1, T2>)obj;

    public override int GetHashCode() => Left.GetHashCode() ^ Right.GetHashCode();
}
