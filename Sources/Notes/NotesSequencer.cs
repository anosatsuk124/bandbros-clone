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


    public readonly double DetectOffsetSeconds = 0.1f;

    public readonly double DetectOnsetSeconds = -0.1;

    public int CurrentNotesIndex = 0;

    public Vector2 DetectPointPosition => DetectPointNode!.GlobalPosition;

    [Export] public Node2D? DetectPointNode { get; set; }

    public NotesSequencer(ActionHandlerBase actionHandlerBase, ChartTrack chartTrack) : base(actionHandlerBase, chartTrack)
    {
        chartNotes = chartTrack.Notes.Select(note => note is ChartNoteHold hold ? hold : null).Where(note => note is not null).ToArray()!;
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
        Play(delta);
    }

    public void Play(double delta)
    {
        UpdateCurrentNotesIndex();
        judgeNote();
        MoveNotes(delta);
    }

    private void UpdateCurrentNotesIndex()
    {
        var deltaTime = actionHandler.performanceManager!.DeltaTime;
        var currentNote = chartNotes[CurrentNotesIndex];
        if (currentNote is null) return;
        if (currentNote.endTime.ToSeconds() < deltaTime + DetectOffsetSeconds) CurrentNotesIndex++;
    }

    private ChartNoteHold[] chartNotes;

    private void judgeNote()
    {
        var deltaTime = actionHandler.performanceManager!.DeltaTime;

        var actions = actionHandler.PerformingActions;
        if (actions is null) return;

        var currentNote = chartNotes[CurrentNotesIndex];

        var noteKinds = PerformanceActionKindExtension.FromMidiNote(currentNote!.note.Note, actionHandler.Scale);
        var startTime = currentNote.startTime.ToSeconds();
        var endTime = currentNote.endTime.ToSeconds();

        foreach (var action in actions)
        {
            if (action is null) continue;
            foreach (var noteKind in noteKinds)
            {
                if (!action.ActionKind.Equals(noteKind)) continue;

                GameManager.Info($"Current Note: {noteKind}");
                GameManager.Info($"Current Action: {action.ActionKind}");

                if (actionHandler.IsActionJustPressed(action) && deltaTime >= startTime + DetectOnsetSeconds && deltaTime <= startTime + DetectOffsetSeconds)
                {
                    GameManager.Info($"Note {noteKind} is HIT");
                }
                else if (actionHandler.IsActionJustReleased(action) && deltaTime >= endTime + DetectOnsetSeconds && deltaTime <= endTime + DetectOffsetSeconds)
                {
                    GameManager.Info($"Note {noteKind} is RELEASED");
                    Notes[CurrentNotesIndex]!.Visible = false;
                }
                else if (actionHandler.IsActionPressing(action) && deltaTime >= startTime + DetectOnsetSeconds && deltaTime <= endTime + DetectOffsetSeconds)
                {
                    GameManager.Info($"Note {noteKind} is HOLDING");
                }
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
                var note = new Note(actionKind, hold.endTime.Sub(hold.startTime), Velocity, DetectPointPosition, hold.note);
                note.Position = new Vector2((float)hold.startTime.ToSeconds() * Velocity, note.Position.Y) + PostionOffset;
                Parent!.AddChild(note);
                yield return note;
            }

            previouseEndTimeUsec = hold.endTime;
        }
    }
}