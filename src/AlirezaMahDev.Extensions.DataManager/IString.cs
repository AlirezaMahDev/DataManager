namespace AlirezaMahDev.Extensions.DataManager;

public interface IString<TSelf> where TSelf : IString<TSelf>
{
    ReadOnlySpan<char> Span { get; }
    public static abstract implicit operator string(TSelf value);
    public static abstract implicit operator TSelf(string? value);
}