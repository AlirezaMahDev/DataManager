using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly unsafe struct String16 : IEquatable<String16>, IString<String16>
{
    private const int Length = 16;
    private const int Size = Length * sizeof(char);
    public static String16 Empty { get; } = new();

    public static implicit operator string(String16 value)
    {
        return value.ToString();
    }

    public static implicit operator String16(string? value)
    {
        return new(value is null ? [] : value);
    }

    public ReadOnlySpan<char> Span
    {
        get
        {
            fixed (String16* pointer = &this)
            {
                return new(pointer, Length);
            }
        }
    }

    private String16(ReadOnlySpan<char> readOnlySpan)
    {
        if (readOnlySpan.Length > Length)
        {
            throw new ArgumentException($"{readOnlySpan.Length} > {Length}.", nameof(readOnlySpan));
        }

        fixed (String16* pointer = &this)
        {
            var span = new Span<char>(pointer, Length);
            span.Clear();
            readOnlySpan.CopyTo(span);
        }
    }

    public override string ToString()
    {
        return new string(Span).Trim();
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

    public bool Equals(String16 other)
    {
        return Span.SequenceEqual(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IString<String16> other && Span.SequenceEqual(other.Span);
    }

    public static bool operator ==(String16 left, String16 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(String16 left, String16 right)
    {
        return !left.Equals(right);
    }
}