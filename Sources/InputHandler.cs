#nullable enable

namespace BandBrosClone;

using Godot;
using System;
using MeltySynth;

using BandBrosClone.MusicNotation;

[GlobalClass]
public sealed partial class InputHandler : ActionHandlerBase
{
    public override MidiChannel Channel { get => performanceManager!.PlayerChannel; set { performanceManager!.PlayerChannel = value; } }

    public override void _Process(double delta)
    {
        _inputHandler();
    }

    private bool[] _currentPlayingActions = new bool[Enum.GetValues<PerformanceActionKind>().Length];

    private MidiNoteVelocity _currentVelocity = new MidiNoteVelocity(100);

    private void _inputHandler()
    {
        foreach (var action in Enum.GetValues<PerformanceActionKind>())
        {
            var isActionPlaying = _currentPlayingActions[(int)action];
            var isActionJustPressed = Input.IsActionJustPressed(action.ToActionName());
            var isActionReleased = Input.IsActionJustReleased(action.ToActionName());

            if ((isActionPlaying && isActionJustPressed) || (!isActionPlaying && !isActionJustPressed)) continue;

            if (!isActionPlaying)
            {
                PerformHandler(new PerformanceAction(action, isActionJustPressed, isActionReleased, _currentVelocity));
                _currentPlayingActions[(int)action] = true;
            }
            else
            {
                PerformHandler(new PerformanceAction(action, isActionJustPressed, isActionReleased, _currentVelocity));
                _currentPlayingActions[(int)action] = false;
            }
        }
    }
}