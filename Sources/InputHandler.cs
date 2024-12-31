namespace BandBrosClone;

using Godot;
using MeltySynth;

using BandBrosClone.MIDI;

[GlobalClass]
public partial class InputHandler : Node
{
    [Export] public SoundfontPlayer SoundfontPlayer { get; set; }

    private Synthesizer _synthesizer;

    public override void _Ready()
    {
        _synthesizer = SoundfontPlayer.Synthesizer;
    }


    private void _synthWithInputAction(InputEvent @event, string actionName, MidiNote note)
    {
        if (@event.IsActionPressed(actionName))
        {
            _synthesizer.NoteOn(0, note, 100);
        }
        if (@event.IsActionReleased(actionName))
        {
            _synthesizer.NoteOff(0, note);
        }
    }

    private int _sharp = 0;
    private int _octave = 0;

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("B_LEFT"))
        {
            _sharp = 1;
        }
        if (@event.IsActionReleased("B_LEFT"))
        {
            _sharp = 0;
        }

        if (@event.IsActionPressed("B_RIGHT"))
        {
            _octave = 1;
        }
        if (@event.IsActionReleased("B_RIGHT"))
        {
            _octave = 0;
        }

        // L: ↓←↑→
        // R: ←↓→↑ (BYXA)
        _synthWithInputAction(@event, "L_DOWN", MidiNote.C4.Sharp(_sharp).ChangeOctave(_octave));
        _synthWithInputAction(@event, "L_LEFT", MidiNote.D4.Sharp(_sharp).ChangeOctave(_octave));
        _synthWithInputAction(@event, "L_UP", MidiNote.E4.Sharp(_sharp).ChangeOctave(_octave));
        _synthWithInputAction(@event, "L_RIGHT", MidiNote.F4.Sharp(_sharp).ChangeOctave(_octave));

        _synthWithInputAction(@event, "R_LEFT", MidiNote.G4.Sharp(_sharp).ChangeOctave(_octave));
        _synthWithInputAction(@event, "R_DOWN", MidiNote.A4.Sharp(_sharp).ChangeOctave(_octave));
        _synthWithInputAction(@event, "R_RIGHT", MidiNote.B4.Sharp(_sharp).ChangeOctave(_octave));
        _synthWithInputAction(@event, "R_UP", MidiNote.C5.Sharp(_sharp).ChangeOctave(_octave));
    }
}