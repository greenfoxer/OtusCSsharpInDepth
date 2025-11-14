using cache.SimpleStoreSystem;

namespace cache.UnitTests
{
    public class HomeWork1
    {
        //Корректный разбор команды SET с тремя аргументами.
        [Fact]
        public void CommandParserShouldParseCommandWith3Args()
        {
            var commandInfo = CommandParser.Parse("ADD 1234 asdfasdfsd".AsSpan(), ' ');
            Assert.True(commandInfo.IsValid());

            Assert.False(commandInfo.Command == ReadOnlySpan<char>.Empty);
            Assert.False(commandInfo.Key == ReadOnlySpan<char>.Empty);
            Assert.False(commandInfo.Value == ReadOnlySpan<char>.Empty);
        }
        //Корректный разбор команды GET с двумя аргументами.
        [Fact]
        public void CommandParserShouldParseCommandWith2Args()
        {
            var commandInfo = CommandParser.Parse("DEL 1234".AsSpan(), ' ');
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
            var commandInfo = CommandParser.Parse("      DEL  1234".AsSpan(), ' ');

            Assert.True(commandInfo.IsValid());
        }
    }
}
