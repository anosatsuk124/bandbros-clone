namespace BandBrosClone;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BandBrosClone.MusicNotation;
using Godot;

public sealed partial class ChartTrackAutoPerformance : ChartTrackSequencerBase
{
    public ChartTrackAutoPerformance(ActionHandlerBase actionHandler, ChartTrack chartTrack) : base(actionHandler, chartTrack)
    {
    }

    public override void _Ready()
    {
    }



    public override void _Process(double delta)
    {
        deltaTime += delta;
    }

    private double deltaTime = 0;

    public override IEnumerator Play(IEnumerable<ChartNote> notes)
    {
        deltaTime = 0;
        foreach (var note in notes)
        {
            while (deltaTime < note.duration.ToSeconds())
            {
                GameManager.Info($"Channel: {midiChannel}, Waiting for {note.duration.ToSeconds() - deltaTime} seconds");
                yield return null;
            }
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

                        while (deltaTime < hold.endTime.ToSeconds()) yield return null;

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

}