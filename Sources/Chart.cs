namespace BandBrosClone;

#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;
using BandBrosClone.MusicNotation;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using ScaleClass = MusicNotation.Scale;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$typeKind")]
[JsonDerivedType(typeof(ChartNoteOn), "noteOn")]
[JsonDerivedType(typeof(ChartNoteOff), "noteOff")]
[JsonDerivedType(typeof(ChartNoteChangeTempo), "changeTempo")]
[JsonDerivedType(typeof(ChartNoteChangeTimeSignature), "changeTimeSignature")]
[JsonDerivedType(typeof(ChartNoteChangeScale), "changeScale")]
[JsonDerivedType(typeof(ChartNoteChangeInstrument), "changeInstrument")]
public abstract record ChartNote(MidiTime duration);
public sealed record ChartNoteOn(MidiChannel channel, MidiNote note, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteOff(MidiChannel channel, MidiNoteNumber note, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeTempo(MidiTempo MidiTempo, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeTimeSignature(MidiTimeSignature timeSignature, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeScale(ScaleClass scale, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeInstrument(MidiChannel channel, MidiInstrumet instrument, MidiTime duration) : ChartNote(duration);

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

        var tempoMap = midiFile.GetTempoMap();


        foreach (var track in midiFile.GetTrackChunks())
        {
            var chartTrack = new ChartTrack();

            foreach (var midiEvent in track.Events)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(new MidiTimeSpan(midiEvent.DeltaTime) as ITimeSpan, tempoMap);
                var sec = new MidiTime(metricTimeSpan.TotalSeconds);

                if (midiEvent is SetTempoEvent setTempo)
                {
                    chartTrack.AddNote(new ChartNoteChangeTempo(new MidiTempo(setTempo.MicrosecondsPerQuarterNote), sec));
                }
                else if (midiEvent is TimeSignatureEvent timeSignature)
                {
                    chartTrack.AddNote(new ChartNoteChangeTimeSignature(new MidiTimeSignature(timeSignature.Numerator, timeSignature.Denominator), sec));
                }
                else if (midiEvent is NoteOnEvent noteOn)
                {
                    chartTrack.AddNote(new ChartNoteOn(new MidiChannel(noteOn.Channel), new MidiNote(noteOn.NoteNumber, noteOn.Velocity), sec));
                }
                else if (midiEvent is NoteOffEvent noteOff)
                {
                    chartTrack.AddNote(new ChartNoteOff(new MidiChannel(noteOff.Channel), new MidiNoteNumber(noteOff.NoteNumber), sec));
                }
                else if (midiEvent is ProgramChangeEvent programChange)
                {
                    chartTrack.AddNote(new ChartNoteChangeInstrument(new MidiChannel(programChange.Channel), new MidiInstrumet(programChange.ProgramNumber), sec));
                }
            }

            chart.AddTrack(chartTrack);
        }

        return chart;
    }
}
