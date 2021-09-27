using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System;

namespace gbcsharp
{
    public static class NoteUtil
    {
        public static NoteName GetNoteFromString(string representation)
        {
            switch(representation)
            {
            case "C_":
                return NoteName.C;
            case "C#":
                return NoteName.CSharp;
            case "D_":
                return NoteName.D;
            case "D#":
                return NoteName.DSharp;
            case "E_":
                return NoteName.E;
            case "F_":
                return NoteName.F;
            case "F#":
                return NoteName.FSharp;
            case "G_":
                return NoteName.G;
            case "G#":
                return NoteName.GSharp;
            case "A_":
                return NoteName.A;
            case "A#":
                return NoteName.ASharp;
            case "B_":
                return NoteName.ASharp;
            default:
                throw new Exception($"Unknown Note representation: {representation}");
            }
        }
        
        public static SevenBitNumber TransposeByOctaveAndPitch(this NoteName note, int octave, int transposeOctave, int transposePitch)
        {
            return (SevenBitNumber)(Convert.ToInt32(note) + 12 * octave + 12 * transposeOctave + transposePitch);
        }
    }
}