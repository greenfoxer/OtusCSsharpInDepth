using cache.SimpleStoreSystem;

namespace cache.UnitTests
{
    public class HomeWork1
    {
        //Корректный разбор команды SET с тремя аргументами.
        [Fact]
        public void CommandParserShouldParseCommandWith3Args()
        {
            var commandInfo = CommandParser.Parse("ADD user:1234 asdfasdfsd".AsSpan(), ' ');
            Assert.True(commandInfo.IsValid());

            Assert.False(commandInfo.Command == ReadOnlySpan<char>.Empty);
            Assert.False(commandInfo.Key == ReadOnlySpan<char>.Empty);
            Assert.False(commandInfo.Value == ReadOnlySpan<char>.Empty);
        }
        //Корректный разбор команды GET с двумя аргументами.
        [Fact]
        public void CommandParserShouldParseCommandWith2Args()
        {
            var commandInfo = CommandParser.Parse("DEL user:1234".AsSpan(), ' ');
            Assert.True(commandInfo.IsValid());

            Assert.False(commandInfo.Command == ReadOnlySpan<char>.Empty);
            Assert.False(commandInfo.Key == ReadOnlySpan<char>.Empty);
            Assert.True(commandInfo.Value == ReadOnlySpan<char>.Empty);
        }
        //Обработка некорректной команды(например, без ключа).
        [Fact]
        public void CommandParserShouldReturnEmptyCommandAggregatorForIncorrectCommand()
        {
            var commandInfo = CommandParser.Parse("DEL".AsSpan(), ' ' );

            Assert.False(commandInfo.IsValid());
        }
        //Обработка команды с лишними пробелами между аргументами.
        [Fact]
        public void CommandParserShouldParseNotTrimmedCommand()
        {
            var commandInfo = CommandParser.Parse("      DEL  user:1234".AsSpan(), ' ');

            Assert.True(commandInfo.IsValid());
        }
        //Обработка команды с некорректным ID.
        [Fact]
        public void CommandParserShouldNotParseWrongId()
        {
            var commandInfo = CommandParser.Parse("DEL user1234".AsSpan(), ' ');

            Assert.False(commandInfo.IsValid());
            Assert.True(commandInfo.Command == ReadOnlySpan<char>.Empty);
            Assert.True(commandInfo.Key == ReadOnlySpan<char>.Empty);
            Assert.True(commandInfo.Value == ReadOnlySpan<char>.Empty);
        }
        //Обработка команды неверным количеством параметров
        [Theory]
        [InlineData("ADD user:1234 DEADBEEF BEEFDEAD")]
        [InlineData("DEL")]
        [InlineData("DEL user:1234 DEADBEEF")]
        [InlineData("GET user:1234 DEADBEEF BEEFDEAD")]
        [InlineData("STA user:1234")]
        public void CommandParserShouldNotParseTooMuchArgs(string value)
        {
            var commandInfo = CommandParser.Parse(value.AsSpan(), ' ');

            Assert.False(commandInfo.IsValid());
            Assert.True(commandInfo.Command == ReadOnlySpan<char>.Empty);
            Assert.True(commandInfo.Key == ReadOnlySpan<char>.Empty);
            Assert.True(commandInfo.Value == ReadOnlySpan<char>.Empty);
        }
    }
}
