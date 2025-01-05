namespace BandBrosClone;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public ChartTrackSequencerBase(ActionHandlerBase actionHandler, ChartTrack chartTrack)
    {
        this.actionHandler = actionHandler;
        this.chartTrack = chartTrack;
        this.actionHandler.Scale = chartTrack.Scale;
        actionHandler.Channel = chartTrack.Channel;
    }

    public abstract IEnumerator Play(IEnumerable<ChartNote> notes);

    protected abstract void HandleChartNote(ChartNote note);

    public IEnumerator<ChartNote> GetEnumerator()
    {
        foreach (var note in chartTrack.Notes)
        {
            yield return note;
        }
    }
}