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

    public override void _Input(InputEvent @event)
    {
        _InputHandler(@event);
    }

    private void _InputHandler(InputEvent @event)
    {
        foreach (var action in Enum.GetValues<PerformanceActionKind>())
        {
            if (@event.IsActionPressed(action.ToActionName()))
            {
                PerformHandler(new PerformanceAction(action, true, false));
            }
            if (@event.IsActionReleased(action.ToActionName()))
            {
                PerformHandler(new PerformanceAction(action, false, true));
            }
        }
    }
}