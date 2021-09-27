using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace gbcsharp
{
    public static class TrackExtensions
    {
        public static void AddNote(this TrackChunk track, Note note)
        {
            using (NotesManager notesManager = track.ManageNotes())
            {
                NotesCollection notes = notesManager.Notes;
                notes.Add(note);
            }
        }

        public static void AddTimedEvent(this TrackChunk track, MidiEvent midiEvent, long time)
        {
            using(TimedEventsManager manager = track.ManageTimedEvents())
            {
                TimedEventsCollection events = manager.Events;
                events.AddEvent(midiEvent, time);
            }
        }
    }
}