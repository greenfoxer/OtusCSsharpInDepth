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
                    return Command != Span<T>.Empty && Key != Span<T>.Empty && Value != Span<T>.Empty;
                case CommandTypes.Get:
                    return Command != Span<T>.Empty && Key != Span<T>.Empty && Value == Span<T>.Empty;
                case CommandTypes.Delete:
                    return Command != Span<T>.Empty && Key != Span<T>.Empty && Value == Span<T>.Empty;
                case CommandTypes.Statistics:
                    return Command != Span<T>.Empty && Key == Span<T>.Empty && Value == Span<T>.Empty;
                default:
                    return false;
            }
        }

        internal void Clear()
        {
            Command = ReadOnlySpan<T>.Empty;
            Key = ReadOnlySpan<T>.Empty;
            Value = ReadOnlySpan<T>.Empty;
        }
    }
}
