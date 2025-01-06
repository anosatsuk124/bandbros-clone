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
    }

    public void SetScale(Scale scale)
    {
        this.scale = scale;
    }

    public abstract IEnumerator Play(IEnumerable<ChartNote> notes);

    public IEnumerator<ChartNote> GetEnumerator()
    {
        foreach (var note in chartTrack.Notes)
        {
            yield return note;
        }
    }
}