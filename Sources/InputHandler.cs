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

    private bool[] _currentPlayingActions = new bool[Enum.GetValues<PerformanceActionKind>().Length];

    private MidiNoteVelocity _currentVelocity = new MidiNoteVelocity(100);

    public void InputHandle()
    {
        foreach (var action in Enum.GetValues<PerformanceActionKind>())
        {
            var isActionPlaying = _currentPlayingActions[(int)action];
            var isActionPressed = Input.IsActionPressed(action.ToActionName());
            var isActionReleased = !isActionPressed;

            if ((isActionPlaying && isActionPressed) || (!isActionPlaying && !isActionPressed)) continue;

            PerformHandler(new PerformanceAction(action, isActionPressed, isActionReleased, _currentVelocity));
            if (!isActionPlaying)
            {
                _currentPlayingActions[(int)action] = true;
            }
            else
            {
                _currentPlayingActions[(int)action] = false;
            }
        }
    }
}