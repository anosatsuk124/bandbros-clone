namespace BandBrosClone;

using System;
using System.Collections;
using System.Collections.Generic;
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

                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {midiChannel}, Press Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, true, false, hold.note.Velocity));
                    }

                    GetTree().CreateTimer(hold.endTime.Sub(hold.startTime).ToSeconds()).Timeout += () =>
                        {
                            var prevscale = scale with { };
                            SetScale(currentScale);
                            foreach (var actionKind in actionKinds)
                            {
                                GameManager.Info($"Channel: {midiChannel}, Release Action: {actionKind.ToActionName()}");
                                actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true, hold.note.Velocity));
                            }
                            SetScale(prevscale);
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