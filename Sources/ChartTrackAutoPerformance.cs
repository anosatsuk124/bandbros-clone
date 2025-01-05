namespace BandBrosClone;

using System;
using System.Collections;
using System.Collections.Generic;
using BandBrosClone.MusicNotation;
using Godot;

public sealed partial class ChartTrackAutoPerformance : ChartTrackSequencerBase
{
    private Timer _timer;

    public ChartTrackAutoPerformance(ActionHandlerBase actionHandler, ChartTrack chartTrack) : base(actionHandler, chartTrack)
    {
    }

    public override void _Ready()
    {
        _timer = new Timer();
        _timer.OneShot = true;
        AddChild(_timer);
    }

    private ulong previousTimeUsec;
    private ulong previousDurationUsec;

    private void _Init()
    {
        actionHandler.Scale = chartTrack.Scale;
        previousTimeUsec = _performanceManager.CurrentTimeUsec;
        previousDurationUsec = 0;
    }


    private Action _play(ulong currentDurationUsec, ChartNote note)
    {
        return () =>
        {
            previousTimeUsec = _performanceManager.CurrentTimeUsec;
            previousDurationUsec = currentDurationUsec;
            HandleChartNote(note);
        };
    }

    public override IEnumerator Play(IEnumerable<ChartNote> notes)
    {
        _Init();
        foreach (var note in notes)
        {
            var timeoffset = previousTimeUsec;
            var currentDurationUsec = note.duration.time;
            var waitTimeOffset = previousTimeUsec - timeoffset;
            GameManager.Info($"Current Time Offset sec: {timeoffset / 1_000_000.0}");

            var play = _play(currentDurationUsec, note);

            if (previousDurationUsec != currentDurationUsec && waitTimeOffset < currentDurationUsec)
            {
                var waitTime = (currentDurationUsec + waitTimeOffset) / 1_000_000.0;
                GameManager.Info($"Waiting for {waitTime} seconds");
                GetTree().CreateTimer(waitTime).Timeout += play;
                yield return null;
            }
            else
            {
                play();
            }
            yield return null;
        }
    }

    protected override void HandleChartNote(ChartNote note)
    {
        switch (note)
        {
            case ChartNoteOn on:
                {
                    var actionKinds = PerformanceActionKindExtension.FromMidiNote(on.note.Note, actionHandler.Scale);
                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {actionHandler.Channel}, Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, true, false), on.note.Velocity);
                    }
                    foreach (var actionKind in actionKinds)
                    {
                        if (actionKind is PerformanceActionKind.SHARP || actionKind is PerformanceActionKind.OCTAVE_UP)
                        {
                            GameManager.Info($"Channel: {actionHandler.Channel}, Action: {actionKind.ToActionName()}");
                            actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true));
                        }
                    }
                    break;
                }
            case ChartNoteOff off:
                {
                    var actionKinds = PerformanceActionKindExtension.FromMidiNote(off.note.Note, actionHandler.Scale);
                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {actionHandler.Channel}, Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true));
                    }
                    break;
                }
            case ChartNoteChangeInstrument changeInstrument:
                {
                    _performanceManager.SetInstrument(actionHandler.Channel, changeInstrument.instrument.bank, changeInstrument.instrument.program);
                    GameManager.Info($"Channel: {actionHandler.Channel}, Instrument: {changeInstrument.instrument}");
                    break;
                }
            default:
                {
                    GameManager.Warn($"Channel: {actionHandler.Channel}, Unhandled note type: {note.GetType().Name}");
                    break;
                }
        }
    }
}