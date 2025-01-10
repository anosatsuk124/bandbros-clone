namespace BandBrosClone;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BandBrosClone.MusicNotation;
using Godot;

public sealed partial class ChartTrackAutoPerformance : ChartTrackSequencerBase
{
    private IEnumerator enumerator;

    public ChartTrackAutoPerformance(ActionHandlerBase actionHandler, ChartTrack chartTrack) : base(actionHandler, chartTrack)
    {
    }

    public override void _Ready()
    {
        enumerator = Play(chartTrack.Notes);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        enumerator.MoveNext();
    }


    public IEnumerator Play(IEnumerable<ChartNote> notes)
    {
        foreach (var note in notes)
        {
            var currentDuration = note.duration.ToSeconds();

            while (_performanceManager.DeltaTime < currentDuration) yield return null;
            HandleChartNote(note);
        }
    }

    public void HandleChartNote(ChartNote note)
    {
        switch (note)
        {
            case ChartNoteHold hold:
                {
                    var currentScale = scale with { };
                    var actionKinds = PerformanceActionKindExtension.FromMidiNote(hold.note.Note, currentScale);

                    if (!actionKinds.Contains(PerformanceActionKind.SHARP))
                    {
                        actionHandler.PerformHandler(new PerformanceAction(PerformanceActionKind.SHARP, false, true, hold.note.Velocity));
                    }
                    if (!actionKinds.Contains(PerformanceActionKind.OCTAVE_UP))
                    {
                        actionHandler.PerformHandler(new PerformanceAction(PerformanceActionKind.OCTAVE_UP, false, true, hold.note.Velocity));
                    }

                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {midiChannel}, Press Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, true, false, hold.note.Velocity));
                    }

                    GetTree().CreateTimer(hold.endTime.Sub(hold.startTime).ToSeconds()).Timeout += () =>
                        {
                            foreach (var actionKind in actionKinds)
                            {
                                GameManager.Info($"Channel: {midiChannel}, Release Action: {actionKind.ToActionName()}");
                                actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true, hold.note.Velocity));
                            }
                        };

                    break;
                }
                //            default:
                //                {
                //                    GameManager.Warn($"Channel: {midiChannel}, Unhandled note type: {note.GetType().Name}");
                //                    break;
                //                }
        }
    }
}