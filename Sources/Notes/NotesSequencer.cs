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
    public List<Note?[]> Notes { get; } = new List<Note?[]>();

    public Node2D? Parent { get; set; }


    public readonly double DetectOffsetSeconds = 0.02;


    public int CurrentNotesIndex = 0;

    public double DeltaTime
    {
        get => actionHandler.performanceManager!.DeltaTime;
    }

    [Export] public Node2D? DetectPointNode { get; set; }

    public NotesSequencer(ActionHandlerBase actionHandlerBase, ChartTrack chartTrack) : base(actionHandlerBase, chartTrack)
    {
    }

    public float Velocity { get; private set; } = 4000;

    public override void _Ready()
    {
        base._Ready();
        Parent!.Scale = new Vector2(0.1f, 0.1f);
        _Init();
    }

    public override void _PhysicsProcess(double delta)
    {
        Play(delta);
    }

    public void Play(double delta)
    {
        var deltaTime = DeltaTime;
        if (CurrentNotesIndex >= Notes.Count)
        {
            return;
        }
        judgeNote(deltaTime);
        MoveNotes(delta, deltaTime);
        _updateCurrentNotesIndex(deltaTime);
    }


    private bool _updateCurrentNotesIndex(double deltaTime)
    {
        var currentNotes = Notes[CurrentNotesIndex];
        if (currentNotes.Count().Equals(0))
        {
            CurrentNotesIndex++;
            return true;
        }
        var lastEndTime = currentNotes.Where(n => n is not null).Select(note => note!.chartNote.endTime.ToSeconds()).Max();
        if (deltaTime >= lastEndTime)
        {
            for (int i = 0; i < currentNotes.Length; i++)
            {
                var note = currentNotes[i];
                if (note is null) continue;
                if (note.HasReleased)
                {
                    note.Visible = false;
                    note.QueueFree();
                    currentNotes[i] = null;
                }
            }
            CurrentNotesIndex++;
        }
        return true;
    }

    private void judgeNote(double deltaTime)
    {
        var actions = actionHandler.PerformingActions;
        if (actions is null) return;

        Note[] currentNotes = Notes[CurrentNotesIndex]!;
        if (currentNotes is null) return;

        for (int i = 0; i < currentNotes.Length; i++)
        {
            foreach (var action in actions)
            {
                if (action is null) continue;
                Note note = currentNotes[i];
                var noteKind = note.actionKind;
                var startTime = note.chartNote.startTime.ToSeconds();
                var endTime = note.chartNote.endTime.ToSeconds();

                //var canAttack = (deltaTime >= startTime - DetectOffsetSeconds) ||
                //               (deltaTime <= startTime + DetectOffsetSeconds);
                var canAttack = (deltaTime >= startTime) ||
                                (deltaTime <= startTime + DetectOffsetSeconds);
                var canRelease = (deltaTime >= endTime - DetectOffsetSeconds) ||
                                 (deltaTime <= endTime + DetectOffsetSeconds);
                // var canRelease = (deltaTime >= endTime - DetectOffsetSeconds) ||
                //                  (deltaTime <= endTime);

                if (note.HasReleased) continue;

                GameManager.Info($"Current Note: {noteKind}");
                GameManager.Info($"Current Action: {action.ActionKind}");
                //                GameManager.Info(actionHandler.IsActionJustPressed(action) ? "Just Pressed" : "Not Just Pressed");
                //                GameManager.Info(actionHandler.IsActionJustReleased(action) ? "Just Released" : "Not Just Released");

                if (!note.IsHolding && actionHandler.IsActionJustPressed(action) && canAttack)
                {
                    GameManager.Info($"Note {noteKind} is HIT");
                    note.IsHolding = true;
                    if (endTime - startTime <= 0.2)
                    {
                        GameManager.Info($"Note {noteKind} is RELEASED, time: {endTime - startTime}");
                        note.IsHolding = false;
                        note.HasReleased = true;
                    }
                }
                else if (note.IsHolding && actionHandler.IsActionJustReleased(action) && !canRelease)
                {
                    GameManager.Info($"Note {noteKind} is MISSED");
                    note.IsHolding = false;
                    note.HasReleased = true;
                }
                else if (note.IsHolding && actionHandler.IsActionJustReleased(action) && canRelease)
                {
                    GameManager.Info($"Note {noteKind} is RELEASED");
                    note.IsHolding = false;
                    note.HasReleased = true;
                }
            }
        }

    }

    public void MoveNotes(double delta, double deltaTime)
    {
        if (deltaTime < 0) return;
        for (int idx = 0; idx < Notes.Count; idx++)
        {
            var notes = Notes[idx];
            foreach (var note in notes)
            {
                if (note is null) continue;
                note.MoveNote(delta);
            }
        }
    }


    private void _Init()
    {
        var player = Play(chartTrack.Notes);
        while (player.MoveNext())
        {
            var noteBase = player.Current;
            if (noteBase is Note[] note)
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
                previouseEndTimeUsec = hold.endTime;
                continue;
            }

            var actionKinds = PerformanceActionKindExtension.FromMidiNote(hold.note.Note, hold.scale);
            var noteNodes = new Note[actionKinds.Length];
            foreach (var (idx, actionKind) in actionKinds.Select((actionKind, idx) => (idx, actionKind)))
            {
                var note = new Note(actionKind, hold.endTime.Sub(hold.startTime), Velocity, hold, chartTrack.Tempo);
                Parent!.AddChild(note);
                note.Position += new Vector2((float)hold.startTime.ToSeconds() * Velocity, 0);
                noteNodes[idx] = note;
            }
            yield return noteNodes;

            previouseEndTimeUsec = hold.endTime;
        }
    }
}