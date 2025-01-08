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

    public override void _PhysicsProcess(double delta)
    {
        if (enumerator.MoveNext())
        {
            return;
        }
    }


    public override IEnumerator Play(IEnumerable<ChartNote> notes)
    {
        foreach (var note in notes)
        {
            var currentDuration = note.duration.ToSeconds();
            var timeoffset = _performanceManager.DeltaTime;

            if (timeoffset < currentDuration)
            {
                while (_performanceManager.DeltaTime < currentDuration) yield return null;
            }
            HandleChartNote(note);
        }
    }

    public void HandleChartNote(ChartNote note)
    {
        switch (note)
        {
            case ChartNoteHold hold:
                {
                    var prevScale = scale with { };
                    var currentScale = hold.scale with { };
                    var actionKinds = PerformanceActionKindExtension.FromMidiNote(hold.note.Note, currentScale);

                    SetScale(currentScale);
                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {midiChannel}, Press Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, true, false, hold.note.Velocity));
                    }
                    SetScale(prevScale);

                    GetTree().CreateTimer(hold.endTime.Sub(hold.startTime).ToSeconds()).Timeout += () =>
                        {
                            SetScale(currentScale);
                            foreach (var actionKind in actionKinds)
                            {
                                GameManager.Info($"Channel: {midiChannel}, Release Action: {actionKind.ToActionName()}");
                                actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true, hold.note.Velocity));
                            }
                            SetScale(prevScale);
                        };

                    break;
                }
            case ChartNoteChangeInstrument changeInstrument:
                {
                    _performanceManager.SetInstrument(midiChannel, changeInstrument.instrument.bank, changeInstrument.instrument.program);
                    GameManager.Info($"Channel: {midiChannel}, Instrument: {changeInstrument.instrument}");
                    break;
                }
            case ChartNoteChangeScale changeScale:
                {
                    var scale = changeScale.scale;
                    SetScale(scale);
                    GameManager.Info($"Channel: {midiChannel}, Scale: {scale}");
                    break;
                }
            default:
                {
                    GameManager.Warn($"Channel: {midiChannel}, Unhandled note type: {note.GetType().Name}");
                    break;
                }
        }
    }
}