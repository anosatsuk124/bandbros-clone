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

    private double initTime;

    private void _Init()
    {
        initTime = _performanceManager.DeltaTime;
    }


    private Action _play(double duration, ChartNote note)
    {
        return async () =>
        {
            await HandleChartNote(note);
        };
    }

    public override IEnumerator Play(IEnumerable<ChartNote> notes)
    {
        _Init();
        foreach (var note in notes)
        {
            var currentDuration = note.duration.ToSeconds();
            var timeoffset = _performanceManager.DeltaTime - initTime;

            var play = _play(currentDuration, note);

            if (timeoffset < currentDuration)
            {
                GetTree().CreateTimer(currentDuration - timeoffset).Timeout += play;
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
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, true, false, hold.note.Velocity));
                    }
                    SetScale(prevScale);

                    await ToSignal(GetTree().CreateTimer(hold.endTime.Sub(hold.startTime).ToSeconds()), Timer.SignalName.Timeout);

                    SetScale(currentScale);
                    foreach (var actionKind in actionKinds)
                    {
                        GameManager.Info($"Channel: {midiChannel}, Release Action: {actionKind.ToActionName()}");
                        actionHandler.PerformHandler(new PerformanceAction(actionKind, false, true, hold.note.Velocity));
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