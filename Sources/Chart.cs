namespace BandBrosClone;

#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using BandBrosClone.MusicNotation;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using ScaleClass = MusicNotation.Scale;
using System;


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

    public MidiChannel Channel { get; set; } = new MidiChannel(0);

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

    public void DetectScale()
    {
        var notes = this.Notes
            .Where(note => note is ChartNoteOn)
            .Select(note => note as ChartNoteOn)
            .Select(note => note!.note.Note);

        var scale = ScaleClass.DetectScale(notes);

        if (scale is not null)
        {
            this.Scale = scale;
        }
    }
}

public class ChartTracks
{
    public ChartTrack this[int index] { get => _Tracks[index]; set => _Tracks[index] = value; }
    public int Count { get => _Tracks.Count; }
    public void Add(ChartTrack track)
    {
        if (_Tracks.Count < MAX_TRACKS)
        {
            _Tracks.Add(track);
        }
    }

    public void RemoveAt(int index)
    {
        _Tracks.RemoveAt(index);
    }

    public ChartTracks()
    {
    }

    public List<ChartTrack> _Tracks { get; set; } = new List<ChartTrack>();
    private readonly static int MAX_TRACKS = Constants.MAX_CHART_TRACK_COUNT;
}

public class Chart
{
    public ChartTracks Tracks { get; private set; } = new ChartTracks();

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
            chartTrack.Channel = new MidiChannel(track.Events.Where(e => e is NoteOnEvent).Select(e => (e as NoteOnEvent)!.Channel).FirstOrDefault());
            var currentTime = new MidiTime(0);

            foreach (var midiEvent in track.Events)
            {
                if (midiEvent is SetTempoEvent setTempo)
                {
                    chartTrack.AddNote(new ChartNoteChangeTempo(new MidiTempo(Convert.ToUInt64(setTempo.MicrosecondsPerQuarterNote)), currentTime));
                }
                else if (midiEvent is TimeSignatureEvent timeSignature)
                {
                    chartTrack.AddNote(new ChartNoteChangeTimeSignature(new MidiTimeSignature(timeSignature.Numerator, timeSignature.Denominator), currentTime));
                }
                else if (midiEvent is NoteOnEvent noteOn)
                {
                    chartTrack.AddNote(new ChartNoteOn(new MidiChannel(noteOn.Channel), new MidiNote(noteOn.NoteNumber, noteOn.Velocity), currentTime));
                }
                else if (midiEvent is NoteOffEvent noteOff)
                {
                    chartTrack.AddNote(new ChartNoteOff(new MidiChannel(noteOff.Channel), new MidiNoteNumber(noteOff.NoteNumber), currentTime));
                }
                else if (midiEvent is ProgramChangeEvent programChange)
                {
                    chartTrack.AddNote(new ChartNoteChangeInstrument(new MidiChannel(programChange.Channel), new MidiInstrumet(programChange.ProgramNumber), currentTime));
                }

                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(new MidiTimeSpan(midiEvent.DeltaTime) as ITimeSpan, tempoMap);
                var usec = new MidiTime(Convert.ToUInt64(metricTimeSpan.TotalMicroseconds));

                currentTime = currentTime.Add(usec);
            }

            chartTrack.DetectScale();
            chart.AddTrack(chartTrack);
        }

        return chart;
    }
}
