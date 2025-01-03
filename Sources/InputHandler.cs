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

    public override void _PhysicsProcess(double delta)
    {
        _inputHandler();
    }

    private bool[] _currentPlayingActions = new bool[Enum.GetValues<PerformanceActionKind>().Length];

    private void _inputHandler()
    {
        foreach (var action in Enum.GetValues<PerformanceActionKind>())
        {
            var isActionPlaying = _currentPlayingActions[(int)action];
            var isActionPressed = Input.IsActionPressed(action.ToActionName());

            if ((isActionPlaying && isActionPressed) || (!isActionPlaying && !isActionPressed)) continue;

            if (!isActionPlaying && isActionPressed)
            {
                PerformHandler(new PerformanceAction(action, true, false));
                _currentPlayingActions[(int)action] = true;
            }
            else if (isActionPlaying && !isActionPressed)
            {
                PerformHandler(new PerformanceAction(action, false, true));
                _currentPlayingActions[(int)action] = false;
            }
        }
    }
}