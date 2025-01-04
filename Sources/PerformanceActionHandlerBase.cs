namespace BandBrosClone;
#nullable enable

using Godot;
using MusicNotation;
using System;


[GlobalClass]
public partial class PerformanceActionHandlerBase : Node
{
    [Export] public PerformanceManager? performanceManager;

    private SoundfontPlayer _soundfontPlayer { get => performanceManager!.SoundfontPlayer; }

    public virtual MidiChannel Channel { get; } = new MidiChannel(0);

    public Scale? Scale { get; set; } = null;


    private MidiNote?[] _currentPlayingActions = new MidiNote?[Enum.GetValues<PerformanceActionKind>().Length];

    private void _playNoteWithInputAction(PerformanceAction action, PerformanceActionKind actionKind, MidiNote note)
    {
        if (action.IsActionPressed(actionKind))
        {
            if (_currentPlayingActions[(int)actionKind] is not null)
            {
                _soundfontPlayer.PlayNoteOff(Channel, _currentPlayingActions[(int)actionKind]);
                _currentPlayingActions[(int)actionKind] = null;
            }

            _soundfontPlayer.PlayNoteOn(Channel, note, 100);
            _currentPlayingActions[(int)actionKind] = note;
        }
        if (action.IsActionReleased(actionKind))
        {
            if (_currentPlayingActions[(int)actionKind] is not null)
            {
                _soundfontPlayer.PlayNoteOff(Channel, _currentPlayingActions[(int)actionKind]);
            }

            _soundfontPlayer.PlayNoteOff(Channel, note);
            _currentPlayingActions[(int)actionKind] = null;
        }
    }

    private int _sharp = 0;
    private int _octave = 0;

    private void _modulateWithAction(PerformanceAction action)
    {
        if (action.IsActionPressed(PerformanceActionKind.SHARP))
        {
            _sharp = 1;
        }
        if (action.IsActionReleased(PerformanceActionKind.SHARP))
        {
            _sharp = 0;
        }

        if (action.IsActionPressed(PerformanceActionKind.OCTAVE_UP))
        {
            _octave = 1;
        }
        if (action.IsActionReleased(PerformanceActionKind.OCTAVE_UP))
        {
            _octave = 0;
        }
    }

    private void _onActionOn(PerformanceActionKind actionKind, int velocity)
    {
        PerformHandler(new PerformanceAction(actionKind, true, false), new MidiNoteVelocity(velocity));
    }

    private void _onActionOff(PerformanceActionKind actionKind)
    {
        PerformHandler(new PerformanceAction(actionKind, false, true), new MidiNoteVelocity());
    }

    public void PerformHandler(PerformanceAction action, MidiNoteVelocity velocity)
    {
        if (action.ActionKind.Equals(PerformanceActionKind.SHARP) || action.ActionKind.Equals(PerformanceActionKind.OCTAVE_UP))
        {
            _modulateWithAction(action);
            return;
        }

        var noteNum = action.ToMidiNoteNumber(_sharp, _octave, Scale);
        var note = new MidiNote(noteNum, velocity);

        _playNoteWithInputAction(action, PerformanceActionKind.I, note);
        _playNoteWithInputAction(action, PerformanceActionKind.II, note);
        _playNoteWithInputAction(action, PerformanceActionKind.III, note);
        _playNoteWithInputAction(action, PerformanceActionKind.IV, note);
        _playNoteWithInputAction(action, PerformanceActionKind.V, note);
        _playNoteWithInputAction(action, PerformanceActionKind.VI, note);
        _playNoteWithInputAction(action, PerformanceActionKind.VII, note);
        _playNoteWithInputAction(action, PerformanceActionKind.VIII, note);
    }
}