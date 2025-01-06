namespace BandBrosClone;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        previousTimeUsec = _performanceManager.CurrentTimeUsec;
        previousDurationUsec = 0;
    }


    private Action _play(ulong currentDurationUsec, ChartNote note)
    {
        return async () =>
        {
            previousTimeUsec = _performanceManager.CurrentTimeUsec;
            previousDurationUsec = currentDurationUsec;
            await HandleChartNote(note);
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

            var play = _play(currentDurationUsec, note);

            if (previousDurationUsec != currentDurationUsec && waitTimeOffset < currentDurationUsec)
            {
                var waitTime = (currentDurationUsec + waitTimeOffset) / 1_000_000.0;
                GetTree().CreateTimer(waitTime).Timeout += play;
            }
            else
            {
                play();
            }
            yield return null;
        }
    }

    public async Task HandleChartNote(ChartNote note)
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
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, true, false), hold.note.Velocity);
                    }
                    SetScale(prevScale);

                    await ToSignal(GetTree().CreateTimer(hold.endTime.Sub(hold.startTime).ToSeconds()), Timer.SignalName.Timeout);

                    SetScale(currentScale);
                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {midiChannel}, Release Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true), hold.note.Velocity);
                    }
                    SetScale(prevScale);
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