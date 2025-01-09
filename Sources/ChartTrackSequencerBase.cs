namespace BandBrosClone;

using System.Collections;
using System.Collections.Generic;
using BandBrosClone.MusicNotation;
using Godot;

public abstract partial class ChartTrackSequencerBase : Node
{
    //TODO: Implement BPM, Time Signature, and Scale changes

    public ActionHandlerBase actionHandler { get; private set; }

    public ChartTrack chartTrack { get; private set; }
    protected PerformanceManager _performanceManager { get => actionHandler.performanceManager; }

    public MidiChannel midiChannel
    {
        get => actionHandler.Channel;
        private set => actionHandler.Channel = value;
    }

    public Scale scale
    {
        get => actionHandler.Scale;
        private set => actionHandler.Scale = value;
    }

    public ChartTrackSequencerBase(ActionHandlerBase actionHandler, ChartTrack chartTrack)
    {
        this.actionHandler = actionHandler;
        this.chartTrack = chartTrack;
        this.actionHandler.Scale = chartTrack.Scale;
        actionHandler.Channel = chartTrack.Channel;
        metaEventEnumerator = MetaEventEnumerator(chartTrack.Notes);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        metaEventEnumerator.MoveNext();
    }

    public void SetScale(Scale scale)
    {
        this.scale = scale;
    }

    private IEnumerator metaEventEnumerator;
    public IEnumerator MetaEventEnumerator(IEnumerable<ChartNote> notes)
    {
        foreach (var note in notes)
        {
            var currentDuration = note.duration.ToSeconds();
            var timeoffset = _performanceManager.DeltaTime;

            if (timeoffset < currentDuration)
            {
                while (_performanceManager.DeltaTime < currentDuration) yield return null;
            }
            HandleMetaEvent(note);
        }
    }

    public void HandleMetaEvent(ChartNote note)
    {
        switch (note)
        {
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
        };
    }

    public IEnumerator<ChartNote> GetEnumerator()
    {
        foreach (var note in chartTrack.Notes)
        {
            yield return note;
        }
    }
}