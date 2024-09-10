namespace HentaiMapGen;

public static class Extensions
{
    public static Span<T> RemovePrefix<T>(this Span<T> input, ReadOnlySpan<T> prefix) where T : IEquatable<T>?
    {
        if (input.StartsWith(prefix))
        {
            return input[prefix.Length..];
        }

        return input;
    }
    
    public static ReadOnlySpan<T> RemovePrefix<T>(this ReadOnlySpan<T> input, ReadOnlySpan<T> prefix) where T : IEquatable<T>?
    {
        if (input.StartsWith(prefix))
        {
            return input[prefix.Length..];
        }

        return input;
    }
}