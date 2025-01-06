namespace BandBrosClone;

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BandBrosClone.MusicNotation;
using Godot;

public partial class NotesSequencer : ChartTrackSequencerBase
{
    public List<Note?> Notes { get; } = new List<Note?>();

    public Node2D? Parent { get; set; }

    public Vector2 PostionOffset { get; set; } = new Vector2(0, 0);

    public NotesSequencer(ActionHandlerBase actionHandlerBase, ChartTrack chartTrack) : base(actionHandlerBase, chartTrack)
    {
    }

    public double Velocity { get; set; }

    public override void _Ready()
    {
        base._Ready();
        _Init();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MoveNotes(delta);
    }

    public void MoveNotes(double deltaTime)
    {
        for (int idx = 0; idx < Notes.Count; idx++)
        {
            var note = Notes[idx];
            if (note is null) continue;
            note!.MoveLocalX((float)-deltaTime * 1000);
            if (note!.Position.X < -500)
            {
                note!.QueueFree();
                Notes[idx] = null;
            }
        }
    }


    private void _Init()
    {
        var player = Play(chartTrack.Notes);
        while (player.MoveNext())
        {
            var noteBase = player.Current;
            if (noteBase is Note note)
            {
                Notes.Add(note);
            }
        }
    }

    public override IEnumerator Play(IEnumerable<ChartNote> chartNotes)
    {
        MidiTime previouseEndTimeUsec = new MidiTime(0);
        foreach (var chartNote in chartNotes)
        {
            if (chartNote is not ChartNoteHold) continue;
            var hold = (chartNote as ChartNoteHold)!;

            if (hold.startTime.time < previouseEndTimeUsec.time)
            {
                GameManager.Warn($"Note {hold.note} at {hold.startTime} is overlapping with previous note at {previouseEndTimeUsec}");
                continue;
            }

            var actionKinds = PerformanceActionKindExtension.FromMidiNote(hold.note.Note, actionHandler.Scale);
            foreach (var actionKind in actionKinds)
            {
                if (actionKind.Equals(PerformanceActionKind.OCTAVE_UP) || actionKind.Equals(PerformanceActionKind.SHARP)) continue;
                var note = new Note(actionKind, new MidiBeat((float)hold.endTime.Sub(hold.startTime).ToSeconds()));
                note.Position = new Vector2((float)hold.startTime.ToSeconds() * 1000, note.Position.Y) + PostionOffset;
                Parent!.AddChild(note);
                yield return note;
            }

            previouseEndTimeUsec = hold.endTime;
        }
    }
}