namespace AlirezaMahDev.Extensions.DataManager;

public interface IString<TValue> where TValue : IString<TValue>
{
    ReadOnlySpan<char> Span { get; }
    public static abstract implicit operator string(TValue value);
    public static abstract implicit operator TValue(string? value);
}