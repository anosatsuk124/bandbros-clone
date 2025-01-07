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


    public readonly float DetectPointOffset = 125f / 4;

    public readonly float DetectPointOnset = -125f / 4;

    public int CurrentNotesIndex = 0;

    public Vector2 DetectPointPosition => DetectPointNode!.GlobalPosition;

    [Export] public Node2D? DetectPointNode { get; set; }

    public NotesSequencer(ActionHandlerBase actionHandlerBase, ChartTrack chartTrack) : base(actionHandlerBase, chartTrack)
    {
    }

    public float Velocity { get; private set; } = 10000;

    public override void _Ready()
    {
        base._Ready();
        Parent!.Scale = new Vector2(0.1f, 0.1f);
        _Init();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        judgeNote();
        UpdateCurrentNotesIndex();
        MoveNotes(delta);
    }

    private void UpdateCurrentNotesIndex()
    {
        var currentNote = Notes[CurrentNotesIndex];
        if (currentNote is null)
        {
            CurrentNotesIndex++;
            return;
        }
        if (currentNote.ReleasePosition.X <= 0)
        {
            CurrentNotesIndex++;
        }
    }

    private void judgeNote()
    {
        var actions = actionHandler.PerformingActions;
        var currentNote = Notes[CurrentNotesIndex];
        if (actions is null) return;

        foreach (var action in actions)
        {
            if (currentNote is null) return;
            if (action is null) continue;
            if (action.ActionKind.Equals(PerformanceActionKind.OCTAVE_UP) || action.ActionKind.Equals(PerformanceActionKind.SHARP)) continue;
            if (!action.ActionKind.Equals(currentNote.actionKind)) continue;

            GameManager.Info($"Current Note: {currentNote!.actionKind}");
            GameManager.Info($"Current Action: {action.ActionKind}");

            if (actionHandler.IsActionJustPressed(action) && currentNote.IsAttack(DetectPointOnset, DetectPointOffset))
            {
                GameManager.Info($"Note {currentNote.actionKind} is HIT");
            }
            else if (actionHandler.IsActionJustReleased(action) && currentNote.IsJustRelease(DetectPointOnset, DetectPointOffset))
            {
                GameManager.Info($"Note {currentNote.actionKind} is RELEASED");
                currentNote.Visible = false;
            }
            else if (actionHandler.IsActionPressing(action) && currentNote.IsHold(DetectPointOnset, DetectPointOffset))
            {
                GameManager.Info($"Note {currentNote.actionKind} is HOLDING");
            }
        }
    }

    public void MoveNotes(double deltaTime)
    {
        for (int idx = 0; idx < Notes.Count; idx++)
        {
            var note = Notes[idx];
            if (note is null) continue;
            note.MoveNote(deltaTime);
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
                var note = new Note(actionKind, new MidiBeat((float)hold.endTime.Sub(hold.startTime).ToSeconds() * Velocity), Velocity, DetectPointPosition);
                note.Position = new Vector2((float)hold.startTime.ToSeconds() * Velocity, note.Position.Y) + PostionOffset;
                Parent!.AddChild(note);
                yield return note;
            }

            previouseEndTimeUsec = hold.endTime;
        }
    }
}