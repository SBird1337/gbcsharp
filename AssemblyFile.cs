using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Serilog;

namespace gbcsharp
{
    public class AssemblyFile
    {
        private const string SYMBOL_REGEX = @"([\w_]+)\:";
        private Dictionary<string,int> _symbolDictionary;
        private Regex _symbolRegex;
        private string[] _lines;

        public string[] Lines { get => _lines; }

        private AssemblyFile(string[] lines)
        {
            _lines = lines;
            _symbolRegex = new Regex(SYMBOL_REGEX, RegexOptions.Compiled);
            _symbolDictionary = ParseAssemblySymbols(_lines);
            
        }
        public static AssemblyFile FromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);
            return new AssemblyFile(lines);
        }

        public int GetLineFromSymbolName(string symbolName)
        {
            return _symbolDictionary[symbolName];
        }

        public int GetChannelCount()
        {
            string channelLine = _lines[1].Trim();
            if(!channelLine.StartsWith("channel_count"))
                throw new Exception("Expected channel_count in line 1");
            return int.Parse(channelLine.Substring(13).Trim());
        }

        public string[] GetChannelSymbols()
        {
            int count = GetChannelCount();
            string[] output = new string[count];
            for(int i = 2; i < 2 + count; ++i)
            {
                string channelDefLine = _lines[i].Trim();
                if(!channelDefLine.StartsWith("channel"))
                    throw new Exception($"Expected channel in line {i}, got {channelDefLine}");
                channelDefLine = channelDefLine.Substring(7).TrimStart();
                string[] split = channelDefLine.Split(",");
                int channelNumber = int.Parse(split[0].Trim());
                string symbol = split[1].Trim();
                output[channelNumber-1] = symbol;
            }
            return output;
        }


        private Dictionary<string, int> ParseAssemblySymbols(string[] lines)
        {
            Dictionary<string, int> outputDictionary = new Dictionary<string, int>();
            for(int i = 0; i < lines.Length; ++i)
            {
                Match m = _symbolRegex.Match(lines[i]);
                if(m.Success)
                    outputDictionary[m.Groups[1].Value] = i;
            }
            return outputDictionary;
        }

        public bool IsSymbol(string line)
        {
            Match m = _symbolRegex.Match(line);
            return m.Success;
        }
    }
}
