namespace BandBrosClone;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BandBrosClone.MusicNotation;
using Godot;

public sealed partial class ChartTrackInputPerformance : ChartTrackSequencerBase
{

    public InputHandler inputHandler { get; private set; }

    public ChartTrackInputPerformance(InputHandler actionHandler, ChartTrack chartTrack) : base(actionHandler, chartTrack)
    {
        inputHandler = actionHandler;
    }

    public override void _Ready()
    {
        base._Ready();
        _performanceManager.PlayerChannel = chartTrack.Channel;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        inputHandler.InputHandle();
    }
}