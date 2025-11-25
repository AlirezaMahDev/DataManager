using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly unsafe struct String64 : IEquatable<String64>, IString<String64>
{
    private const int Size = Length * sizeof(char);
    private const int Length = 64;

    public static String64 Empty { get; } = new();

    public static implicit operator string(String64 value)
    {
        return value.ToString();
    }

    public static implicit operator String64(string? value)
    {
        return value is null ? Empty : new(value.PadRight(Length, ' ').AsSpan());
    }

    public ReadOnlySpan<char> Span
    {
        get
        {
            fixed (String64* pointer = &this)
            {
                return new(pointer, Length);
            }
        }
    }

    private String64(ReadOnlySpan<char> readOnlySpan)
    {
        if (readOnlySpan.Length > Length)
        {
            throw new ArgumentException($"{readOnlySpan.Length} > {Length}.", nameof(readOnlySpan));
        }

        fixed (String64* pointer = &this)
        {
            var span = new Span<char>(pointer, Length);
            span.Clear();
            readOnlySpan.CopyTo(span);
        }
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.AddBytes(MemoryMarshal.AsBytes(Span));
        return hashCode.ToHashCode();
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    public override string ToString()
    {
        return new string(Span).Trim();
    }

    public bool Equals(String64 other)
    {
        return Span.SequenceEqual(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IString<String64> other && Span.SequenceEqual(other.Span);
    }

    public static bool operator ==(String64 left, String64 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(String64 left, String64 right)
    {
        return !left.Equals(right);
    }
}