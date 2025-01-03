namespace BandBrosClone;

#nullable enable

using System.Collections.Generic;
using System.Xml.Serialization;
using BandBrosClone.MusicNotation;
using Melanchall.DryWetMidi.Core;
using ScaleClass = MusicNotation.Scale;

public abstract record ChartNote;
public sealed record ChartNoteHold(MidiChannel channel, MidiNote note, MidiTime duration) : ChartNote;
public sealed record ChartNoteRest(MidiTime duration) : ChartNote;
public sealed record ChartNoteChangeTempo(MidiTempo MidiTempo) : ChartNote;
public sealed record ChartNoteChangeTimeSignature(MidiTimeSignature timeSignature) : ChartNote;
public sealed record ChartNoteChangeScale(ScaleClass scale) : ChartNote;
public sealed record ChartNoteChangeInstrument(MidiChannel channel, MidiInstrumet instrument) : ChartNote;

public class ChartTrack
{
    public MidiTempo Tempo { get; set; } = Constants.DEFAULT_TEMPO;

    public MidiTimeSignature TimeSignature { get; set; } = Constants.DEFAULT_TIME_SIGNATURE;

    public ScaleClass Scale { get; set; } = Constants.DEFAULT_SCALE;

    public List<ChartNote> Notes { get; set; } = new List<ChartNote>();

    public ChartTrack()
    {
    }

    public ChartTrack(MidiTempo tempo, MidiTimeSignature timeSignature, ScaleClass scale)
    {
        this.Tempo = tempo;
        this.TimeSignature = timeSignature;
        this.Scale = scale;
    }

    public void AddNote(ChartNote note)
    {
        this.Notes.Add(note);
    }

    public void RemoveNoteAt(int index)
    {
        this.Notes.RemoveAt(index);
    }
}

public class Chart
{
    public List<ChartTrack> Tracks { get; private set; } = new List<ChartTrack>();

    public Chart()
    {
    }

    public void AddTrack(ChartTrack track)
    {
        this.Tracks.Add(track);
    }

    public void RemoveTrackAt(int index)
    {
        this.Tracks.RemoveAt(index);
    }

    public static Chart? CreateChartFromMidiFile(MidiFile midiFile)
    {
        var chart = new Chart();

        foreach (var track in midiFile.GetTrackChunks())
        {
            var chartTrack = new ChartTrack();

            Dictionary<MidiNote, ChartNoteHold> currentNote2Time = new();

            foreach (var midiEvent in track.Events)
            {
                foreach (var note2Time in currentNote2Time)
                {
                    // Add the delta time to the duration of the note to keep track of the time it has been playing.
                    currentNote2Time[note2Time.Key] = note2Time.Value with { duration = note2Time.Value.duration.Add(midiEvent.DeltaTime) };
                }

                if (midiEvent is SetTempoEvent setTempo)
                {
                    chartTrack.AddNote(new ChartNoteChangeTempo(new MidiTempo(setTempo.MicrosecondsPerQuarterNote)));
                }
                else if (midiEvent is TimeSignatureEvent timeSignature)
                {
                    chartTrack.AddNote(new ChartNoteChangeTimeSignature(new MidiTimeSignature(timeSignature.Numerator, timeSignature.Denominator)));
                }
                else if (midiEvent is NoteOnEvent noteOn)
                {
                    currentNote2Time.TryAdd(new MidiNote(noteOn.NoteNumber), new ChartNoteHold(new MidiChannel(noteOn.Channel), new MidiNote(noteOn.NoteNumber, noteOn.Velocity), new MidiTime(0)));
                }
                else if (midiEvent is NoteOffEvent noteOff)
                {
                    if (currentNote2Time.TryGetValue(new MidiNote(noteOff.NoteNumber), out var note))
                    {
                        chartTrack.AddNote(note);
                        currentNote2Time.Remove(new MidiNote(noteOff.NoteNumber));
                    }
                }
                else if (midiEvent is ProgramChangeEvent programChange)
                {
                    chartTrack.AddNote(new ChartNoteChangeInstrument(new MidiChannel(programChange.Channel), new MidiInstrumet(programChange.ProgramNumber)));
                }
            }

            chart.AddTrack(chartTrack);
        }

        return chart;
    }
}
