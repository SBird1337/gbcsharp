using Serilog;
using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using System;

namespace gbcsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            AssemblyFile file = AssemblyFile.FromFile("../seelstage.asm");
            List<TrackChunk> chunks = new List<TrackChunk>();

            chunks.Add(new TrackChunk()); //Contains Tempo Information
            string[] channelSymbols = file.GetChannelSymbols();
            for(int i = 0; i < channelSymbols.Length; ++i)
                chunks.Add(new TrackChunk());
            for(int i = 0; i < channelSymbols.Length; ++i)
            {
                TrackParser parser = new TrackParser(file, i);
                parser.ParseTrack(chunks);
            }

            MidiFile midi = new MidiFile(chunks);
            midi.Write("test.mid", true);
            
        }
    }
}
