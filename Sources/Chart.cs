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
[JsonDerivedType(typeof(ChartNoteHold), "noteHold")]
[JsonDerivedType(typeof(ChartNoteChangeTempo), "changeTempo")]
[JsonDerivedType(typeof(ChartNoteChangeTimeSignature), "changeTimeSignature")]
[JsonDerivedType(typeof(ChartNoteChangeScale), "changeScale")]
[JsonDerivedType(typeof(ChartNoteChangeInstrument), "changeInstrument")]
[JsonDerivedType(typeof(ChartNoteChangeKeySignature), "changeKeySignature")]
public abstract record ChartNote(MidiTime duration);
public sealed record ChartNoteHold(MidiChannel channel, MidiNote note, ScaleClass scale, MidiTime startTime, MidiTime endTime) : ChartNote(startTime);
public sealed record ChartNoteChangeTempo(MidiTempo MidiTempo, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeTimeSignature(MidiTimeSignature timeSignature, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeScale(ScaleClass scale, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeInstrument(MidiChannel channel, MidiInstrumet instrument, MidiTime duration) : ChartNote(duration);
public sealed record ChartNoteChangeKeySignature(ScaleClass scale, MidiTime duration) : ChartNote(duration);

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

    public void InsertNoteAt(int index, ChartNote note)
    {
        this.Notes.Insert(index, note);
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
            .Where(note => note is ChartNoteHold)
            .Select(note => note as ChartNoteHold)
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
        _Tracks.Add(track);
    }

    public void RemoveAt(int index)
    {
        _Tracks.RemoveAt(index);
    }

    public ChartTracks()
    {
    }

    public List<ChartTrack> _Tracks { get; set; } = new List<ChartTrack>();
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
            List<ScaleClass> scales = [Constants.DEFAULT_SCALE];

            if (chartTrack.Channel != 10)  // Channel 10 is reserved for percussion
            {
                var notes = track.Events.Select(e => e is NoteOnEvent noteOn ? new MidiNoteNumber(noteOn.NoteNumber) : null).Where(e => e is not null).Select(e => e!);
                var newScale = ScaleClass.DetectScale(notes);
                if (newScale is not null)
                {
                    scales[0] = newScale;
                    chartTrack.AddNote(new ChartNoteChangeScale(newScale, currentTime));
                    chartTrack.Scale = newScale;
                }
            }

            (MidiNote, MidiTime)?[] noteIsPlaying = new (MidiNote, MidiTime)?[128];

            foreach (var midiEvent in track.Events)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(new MidiTimeSpan(midiEvent.DeltaTime) as ITimeSpan, tempoMap);
                var usec = new MidiTime(Convert.ToUInt64(metricTimeSpan.TotalMicroseconds));
                currentTime = currentTime.Add(usec);

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
                    var midiNote = new MidiNote(noteOn.NoteNumber, noteOn.Velocity);
                    noteIsPlaying[midiNote.Note] = (midiNote, currentTime);

                }
                else if (midiEvent is NoteOffEvent noteOff)
                {
                    var playing = noteIsPlaying[noteOff.NoteNumber];
                    if (playing is null) continue;

                    var midiNote = playing.Value.Item1;
                    var startTime = playing.Value.Item2;
                    var endTime = currentTime;

                    var scale = scales.Last();
                    var octaveDistance = scale.GetOctaveDistance(midiNote.Note);
                    if (chartTrack.Channel != 10 && (octaveDistance > 1 || octaveDistance < 0))
                    {
                        scale = scale with { Key = scale.Key.ChangeOctave(octaveDistance) };
                        scales.Add(scale);
                        chartTrack.AddNote(new ChartNoteChangeScale(scale, currentTime));
                    }
                    chartTrack.AddNote(new ChartNoteHold(chartTrack.Channel, midiNote, scale, startTime, endTime));
                    noteIsPlaying[midiNote.Note] = null;
                }
                else if (midiEvent is ProgramChangeEvent programChange)
                {
                    chartTrack.AddNote(new ChartNoteChangeInstrument(new MidiChannel(programChange.Channel), new MidiInstrumet(programChange.ProgramNumber), currentTime));
                }
                else if (midiEvent is KeySignatureEvent keySignatureEvent)
                {
                    if (chartTrack.Channel == 10) continue;
                    var scale = scales.Last().UpdateKeySig(keySignatureEvent.Key);
                    chartTrack.AddNote(new ChartNoteChangeKeySignature(scale, currentTime));
                }
            }

            chart.AddTrack(chartTrack);
        }

        return chart;
    }
}
