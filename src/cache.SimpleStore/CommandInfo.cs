namespace cache.SimpleStoreSystem
{
    public enum CommandTypes
    {
        Unknown = 0,
        Add,
        Get,
        Delete,
        Statistics
    }
    public ref struct CommandInfo<T> where T : IEquatable<T>
    {
        public CommandTypes CommandType { get; set; }
        public ReadOnlySpan<T> Command;
        public ReadOnlySpan<T> Key;
        public ReadOnlySpan<T> Value;

        public readonly bool IsValid()
        {
            switch (CommandType)
            {
                case CommandTypes.Add:
                    return Key != Span<T>.Empty && Value != Span<T>.Empty;
                case CommandTypes.Get:
                    return Key != Span<T>.Empty && Value == Span<T>.Empty;
                case CommandTypes.Delete:
                    return Key != Span<T>.Empty && Value == Span<T>.Empty;
                case CommandTypes.Statistics:
                    return Key == Span<T>.Empty && Value == Span<T>.Empty;
                default:
                    return false;
            }
        }
    }
}
