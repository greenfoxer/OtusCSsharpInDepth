using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace cache.SimpleStoreSystem
{
    public static class CommandParser
    {
        public static CommandInfo<char> Parse(ReadOnlySpan<char> payload, char delimeter)
        {
            ReadOnlySpan<char> window = payload;
            var commandInfo = new CommandInfo<char>();
            // find command 
            EnrichCommand(ref commandInfo, ref window, delimeter);
            // find key
            EnrichId(ref commandInfo, ref window, delimeter);
           // find value till end
            EnrichValue(ref commandInfo, ref window, delimeter);

            return commandInfo;
        }
        private static ReadOnlyMemory<char> DelCommand = "DEL".AsMemory();
        private static ReadOnlyMemory<char> AddCommand = "ADD".AsMemory();
        private static ReadOnlyMemory<char> GetCommand = "GET".AsMemory();
        private static ReadOnlyMemory<char> StaCommand = "STA".AsMemory();
        private static void EnrichCommand(ref CommandInfo<char> commandInfo, ref ReadOnlySpan<char> payload, char delimeter)
        {
            var rawParsed = ParseInternal(payload, delimeter);
            var position = payload.IndexOf(rawParsed) + rawParsed.Length;
            payload = payload.Slice(position);

            if (rawParsed.Equals(DelCommand.Span, StringComparison.OrdinalIgnoreCase))
            {
                commandInfo.CommandType = CommandTypes.Delete;
            }
            else if (rawParsed.Equals(AddCommand.Span, StringComparison.OrdinalIgnoreCase))
            {
                commandInfo.CommandType = CommandTypes.Add;
            }
            else if (rawParsed.Equals(GetCommand.Span, StringComparison.OrdinalIgnoreCase))
            {
                commandInfo.CommandType = CommandTypes.Get;
            }
            else if (rawParsed.Equals(StaCommand.Span, StringComparison.OrdinalIgnoreCase))
            {
                commandInfo.CommandType = CommandTypes.Statistics;
            }
            if (commandInfo.CommandType != CommandTypes.Unknown)
            {
                commandInfo.Command = rawParsed;
            }
        }
        private static void EnrichId(ref CommandInfo<char> commandInfo, ref ReadOnlySpan<char> payload, char delimeter)
        {
            commandInfo.Key = ParseInternal(payload, delimeter);
            var position = payload.IndexOf(commandInfo.Key) + commandInfo.Key.Length;
            payload = payload.Slice(position);
        }
        private static void EnrichValue(ref CommandInfo<char> commandInfo, ref ReadOnlySpan<char> payload, char delimeter)
        {
            // find datalength if nesse
            //var rawDataLength = ParseInternal(payload, delimeter);
            //if (rawDataLength != Span<char>.Empty)
            //{
            //    var position = payload.IndexOf(rawDataLength) + rawDataLength.Length;
            //    var window = payload.Slice(position);
            //    int.TryParse(rawDataLength, out var dataLength);
            //}
            commandInfo.Value = ParseInternal(payload, delimeter);
            var position = payload.IndexOf(commandInfo.Value) + commandInfo.Value.Length;
            payload = payload.Slice(position);
        }

        private static ReadOnlySpan<char> ParseInternal(ReadOnlySpan<char> source, char delimeter, int? dataLength = null)
        {
            if (source.Length == 0)
                return ReadOnlySpan<char>.Empty;

            ReadOnlySpan<char> window = source;

            // skip to find beginning of payload
            while ( window.IndexOf(delimeter) == 0 )
            {
                window = window.Slice(1, window.Length - 1);
            }

            // find end of payload
            var index = window.IndexOf(delimeter);

            // get payload
            var value = index == -1 ? window : window.Slice(0, index);

            if (dataLength != null && index != dataLength - 1)
                return ReadOnlySpan<char>.Empty;

            return value;
        }
    }
}
