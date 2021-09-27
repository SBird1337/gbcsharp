using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Serilog;
using System.Collections.Generic;

namespace gbcsharp
{
    public class TrackParser
    {
        private const int BPM_PRESCALE = 2;
        private AssemblyFile _assembly;
        private int _channel;
        private int _baseNoteLength;
        private int _transposeOctave;
        private int _transposePitch;

        private long _time;
        private int _octave;
        public TrackParser(AssemblyFile file, int channel)
        {
            _assembly = file;
            _channel = channel + 1;
        }

        public void ParseTrack(List<TrackChunk> tracks)
        {
            string[] channelSymbols = _assembly.GetChannelSymbols();
            int startLine = _assembly.GetLineFromSymbolName(channelSymbols[_channel-1]) + 1;

            _time = 0;
            _octave = 4;
            _baseNoteLength = 1;
            _transposeOctave = 0;
            _transposePitch = 0;

            ParseSegment(startLine, tracks);
        }

        private void ParseSegment(int line, List<TrackChunk> tracks)
        {
            bool done = false;
            while(!done)
            {
                string currentLine = _assembly.Lines[line];
                done = ParseCommand(currentLine, tracks);
                line++;
            }
        }

        private bool ParseCommand(string line, List<TrackChunk> tracks)
        {
            if(_assembly.IsSymbol(line))
                return false;

            line = line.Trim();
            if(line == string.Empty)
                return false;
            
            string command;
            List<string> parameters = new List<string>();
            int whitespaceIndex = line.IndexOf(' ');
            if(whitespaceIndex != -1)
            {
                command = line.Substring(0, line.IndexOf(' '));
                line = line.Substring(command.Length).TrimStart();
                string[] split = line.Split(",");
                for(int i = 0; i < split.Length; ++i)
                    parameters.Add(split[i].Trim());
            }
            else
            {
                command = line;
            }

            switch(command)
            {
            case "tempo":
                {
                    //convert to bpm according to pokecrystal documentation, then convert to microseconds per quarter note
                    double bpm = 19200.0d / (double)int.Parse(parameters[0]);
                    tracks[0].Events.Add(new SetTempoEvent((long)(60000000.0d / bpm)));
                }
                break;
            case "rest":
                {
                    int restTime = int.Parse(parameters[0]);
                    _time += restTime * _baseNoteLength;
                }
                break;
            case "note":
                {
                    Melanchall.DryWetMidi.MusicTheory.NoteName noteName = NoteUtil.GetNoteFromString(parameters[0]);
                    long length = int.Parse(parameters[1]);
                    using (NotesManager notesManager = tracks[_channel].ManageNotes())
                    {
                        NotesCollection notes = notesManager.Notes;
                        notes.Add(new Note(noteName.TransposeByOctaveAndPitch( _octave, _transposeOctave, _transposePitch), length * _baseNoteLength * BPM_PRESCALE, _time * BPM_PRESCALE));
                    }
                    _time += length * _baseNoteLength;
                }
                break;
            case "drum_note":
                {
                    int instrument = int.Parse(parameters[0]); //TODO: Use
                    int length = int.Parse(parameters[1]);
                    using (NotesManager notesManager = tracks[_channel].ManageNotes())
                    {
                        NotesCollection notes = notesManager.Notes;
                        notes.Add(new Note((SevenBitNumber)(60 + instrument), length * _baseNoteLength * BPM_PRESCALE, _time * BPM_PRESCALE));
                    }
                    _time += length * _baseNoteLength;
                }
                break;
            case "drum_speed":
                {
                    _baseNoteLength = int.Parse(parameters[0]);
                }
                break;
            case "octave":
                {
                    _octave = int.Parse(parameters[0]);
                }
                break;
            case "transpose":
                {
                    _transposeOctave = int.Parse(parameters[0]);
                    _transposePitch = int.Parse(parameters[1]);
                }
                break;
            case "sound_call":
                {
                    int calledLine = _assembly.GetLineFromSymbolName(parameters[0]);
                    ParseSegment(calledLine+1, tracks);
                }
                break;
            case "sound_loop":
            case "sound_ret":
                return true;
            case "note_type":
                {
                    _baseNoteLength = int.Parse(parameters[0]);
                    //TODO: fade/wave, volume
                }
                break;
            default:
                Log.Warning($"Unknown Command: {command}");
                break;
            }
            return false;
        }
    }
}
