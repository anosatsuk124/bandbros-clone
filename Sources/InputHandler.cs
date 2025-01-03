#nullable enable

namespace BandBrosClone;

using Godot;
using System;
using MeltySynth;

using BandBrosClone.MusicNotation;

[GlobalClass]
public partial class InputHandler : PerformanceActionHandler
{
    public InputHandler(SoundfontPlayer soundfontPlayer, MidiChannel? channel = null) : base(soundfontPlayer, channel) { }

    public override void _Process(double delta)
    {
        _InputHandler();
    }

    private bool[] _currentPlayingActions = new bool[Enum.GetValues<PerformanceActionKind>().Length];

    private void _InputHandler()
    {
        foreach (var action in Enum.GetValues<PerformanceActionKind>())
        {
            if (!_currentPlayingActions[(int)action] && Input.IsActionPressed(action.ToActionName()))
            {
                PerformHandler(new PerformanceAction(action, true, false));
                _currentPlayingActions[(int)action] = true;
            }
            else if (_currentPlayingActions[(int)action] && !Input.IsActionPressed(action.ToActionName()))
            {
                PerformHandler(new PerformanceAction(action, false, true));
                _currentPlayingActions[(int)action] = false;
            }
        }
    }
}